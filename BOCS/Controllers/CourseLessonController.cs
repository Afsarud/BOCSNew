using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using BOCS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BOCS.Controllers
{
   // Admin only: manage lessons under a course
    [Authorize(Roles = "Admin,Teacher")]
    [Route("admin/course-lessons")]
    [AutoValidateAntiforgeryToken] // POST এ টোকেন চাই
    public class CourseLessonController : Controller
    {
        private readonly AppDbContext _db;
        public CourseLessonController(AppDbContext db) => _db = db;
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .Select(c => new CourseMiniVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    CourseType = c.CourseType,
                    LessonCount = _db.Lessons.Count(l => l.CourseId == c.Id)
                })
                .ToListAsync();

            return View("Index", courses); 
        }

     
        [HttpGet("{courseId:int}", Name = "CourseLessons_Manage")]
        public async Task<IActionResult> Manage(int courseId)
        {
            var course = await _db.Courses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            var vm = new LessonManageVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Lessons = await _db.Lessons
                    .Where(l => l.CourseId == courseId)
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new LessonItemVM
                    {
                        Id = l.Id,
                        Title = l.Title,
                        YoutubeId = l.YoutubeId,
                        SortOrder = l.SortOrder,
                        IsPublished = l.IsPublished,
                        CreatedAtUtc = l.CreatedAtUtc,
                        IsPlay = l.IsPlay
                    })
                    .ToListAsync()
            };

            return View("Manage", vm);
        }

        [HttpPost("tick")]   // POST /admin/course-lessons/tick?courseId=20
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tick(int courseId, [FromBody] TickDto dto)
        {
            if (dto?.Ids == null || dto.Ids.Count == 0) return BadRequest("No ids");

            var lessons = await _db.Lessons
                .Where(x => x.CourseId == courseId && dto.Ids.Contains(x.Id))
                .ToListAsync();

            if (lessons.Count == 0) return NotFound();

            foreach (var l in lessons)
                l.IsPlay = dto.Value;

            await _db.SaveChangesAsync();
            return Ok(new { updated = lessons.Count, value = dto.Value ? 1 : 0 });
        }

        [HttpGet("{courseId:int}/create")]
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            // next sort = max + 1 (খালি হলে 0)
            var max = await _db.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => (int?)l.SortOrder)
                .MaxAsync() ?? -1;
            var next = max + 1;

            ViewBag.Subjects = await _db.Subjects
                .Where(s => s.CourseId == courseId && s.IsPublished)
                .OrderBy(s => s.SortOrder)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                .ToListAsync();
            var vm = new LessonCreateVM
            {
                CourseId = courseId,
                SortOrder = next,
                IsPublished = true
            };

            return View(vm);
        }

        [HttpPost("{courseId:int}/create"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, LessonCreateVM vm)
        {
            if (courseId != vm.CourseId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await _db.Subjects.AsNoTracking()
                    .Where(s => s.CourseId == courseId && s.IsPublished)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                    .ToListAsync();
                return View(vm);
            }
            var max = await _db.Lessons
                       .Where(l => l.CourseId == courseId)
                       .Select(l => (int?)l.SortOrder)
                       .MaxAsync() ?? -1;
            var next = max + 1;

            var ytId = YoutubeHelper.ExtractId(vm.YoutubeUrlOrId);
            if (ytId == null)
                ModelState.AddModelError(nameof(vm.YoutubeUrlOrId), "Invalid YouTube URL or ID.");

            //if (!ModelState.IsValid) return View("Create", vm);

            _db.Lessons.Add(new CourseLesson
            {
                CourseId = courseId,
                Title = vm.Title,
                YoutubeId = ytId,
                YoutubeUrlRaw = vm.YoutubeUrlOrId,
                IsPublished = vm.IsPublished,
                SubjectId = vm.SubjectId,  //new added subject
                SortOrder = next,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "✅ Lesson created.";
            return RedirectToAction(nameof(Manage), new { courseId });
        }

        [HttpGet("{courseId:int}/edit/{id:int}")]
        public async Task<IActionResult> Edit(int courseId, int id)
        {
            var lesson = await _db.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
            if (lesson == null) return NotFound();

            // subject dropdown
            ViewBag.Subjects = await _db.Subjects
                .Where(s => s.CourseId == courseId && s.IsPublished)
                .OrderBy(s => s.SortOrder)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                .ToListAsync();

            var vm = new LessonEditVM
            {
                CourseId = courseId,
                Id = lesson.Id,
                Title = lesson.Title,
                YoutubeUrlOrId = string.IsNullOrWhiteSpace(lesson.YoutubeUrlRaw) ? lesson.YoutubeId : lesson.YoutubeUrlRaw,
                SortOrder = lesson.SortOrder,
                IsPublished = lesson.IsPublished,
                SubjectId = lesson.SubjectId
            };

            return View("Edit", vm);
        }

        [HttpPost("{courseId:int}/edit/{id:int}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int courseId, int id, LessonEditVM vm)
        {
            if (courseId != vm.CourseId || id != vm.Id) return BadRequest();

            var ytId = YoutubeHelper.ExtractId(vm.YoutubeUrlOrId);
            if (ytId == null)
                ModelState.AddModelError(nameof(vm.YoutubeUrlOrId), "Invalid YouTube URL or ID.");

            if (!ModelState.IsValid)
            {
                // repopulate dropdown
                ViewBag.Subjects = await _db.Subjects
                    .Where(s => s.CourseId == courseId && s.IsPublished)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                    .ToListAsync();

                return View("Edit", vm);
            }

            var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
            if (lesson == null) return NotFound();

            lesson.Title = vm.Title;
            lesson.YoutubeId = ytId;
            lesson.YoutubeUrlRaw = vm.YoutubeUrlOrId;
            lesson.SortOrder = vm.SortOrder;
            lesson.IsPublished = vm.IsPublished;
            lesson.SubjectId = vm.SubjectId;

            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "✏️ Lesson updated.";
            return RedirectToAction(nameof(Manage), new { courseId });
        }

        [HttpGet("{courseId:int}/delete/{id:int}")]
        public async Task<IActionResult> Delete(int courseId, int id)
        {
            var l = await _db.Lessons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
            if (l == null) return NotFound();

            return View("Delete", new LessonDeleteVM
            {
                CourseId = courseId,
                Id = id,
                Title = l.Title
            });
        }

        [HttpPost("{courseId:int}/delete/{id:int}"), ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int courseId, int id)
        {
            var l = await _db.Lessons
                .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
            if (l == null) return NotFound();

            _db.Lessons.Remove(l);
            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = "🗑️ Lesson deleted.";
            return RedirectToAction(nameof(Manage), new { courseId });
        }

        //drag and drop start

        public class ReorderDto
        {
            public List<int> Ids { get; set; } = new();
        }

        [HttpPost("reorder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int courseId, [FromBody] ReorderDto dto)
        {
            if (dto?.Ids == null || dto.Ids.Count == 0)
                return BadRequest("No ids");

            var lessons = await _db.Lessons
                .Where(x => x.CourseId == courseId && dto.Ids.Contains(x.Id))
                .ToListAsync();

            if (lessons.Count != dto.Ids.Count)
                return BadRequest("Mismatched ids");

            for (int i = 0; i < dto.Ids.Count; i++)
                lessons.First(x => x.Id == dto.Ids[i]).SortOrder = i; // অথবা i+1

            await _db.SaveChangesAsync();
            return Ok(new { updated = lessons.Count });
        }

    }
}
