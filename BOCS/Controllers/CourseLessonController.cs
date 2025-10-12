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
    [Authorize(Roles = "Admin,Teacher")]
    [Route("admin/course-lessons")]
    [AutoValidateAntiforgeryToken] 
    public class CourseLessonController : Controller
    {
        private readonly AppDbContext _db;
        private readonly FileUploadService _fileUploadService;

        public CourseLessonController(AppDbContext db, FileUploadService fileUploadService)
        {
            _db = db;
            _fileUploadService = fileUploadService;
        }
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
                        IsPlay = l.IsPlay,
                        ImageCount = l.Attachments.Count(a => a.AttachmentType == AttachmentType.Image),
                        FileCount = l.Attachments.Count(a => a.AttachmentType == AttachmentType.Document)
                    })
                    .ToListAsync()
            };

            return View("Manage", vm);
        }

        [HttpPost("tick")]
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
                IsPublished = true,
                ExistingImages = new List<AttachmentDisplayVM>(),
                ExistingDocuments = new List<AttachmentDisplayVM>()
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

                // Initialize attachment collections if they're null
                vm.ExistingImages ??= new List<AttachmentDisplayVM>();
                vm.ExistingDocuments ??= new List<AttachmentDisplayVM>();

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

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await _db.Subjects.AsNoTracking()
                    .Where(s => s.CourseId == courseId && s.IsPublished)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                    .ToListAsync();
                return View(vm);
            }

            var lesson = new CourseLesson
            {
                CourseId = courseId,
                Title = vm.Title,
                YoutubeId = ytId,
                YoutubeUrlRaw = vm.YoutubeUrlOrId,
                IsPublished = vm.IsPublished,
                SubjectId = vm.SubjectId,
                SortOrder = next,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Lessons.Add(lesson);
            await _db.SaveChangesAsync();

            // Handle file uploads
            var imageAttachments = await _fileUploadService.SaveFilesAsync(vm.LessonImages, lesson.Id, AttachmentType.Image);
            var docAttachments = await _fileUploadService.SaveFilesAsync(vm.LessonDocuments, lesson.Id, AttachmentType.Document);

            if (imageAttachments.Any() || docAttachments.Any())
            {
                _db.LessonAttachment.AddRange(imageAttachments);
                _db.LessonAttachment.AddRange(docAttachments);
                await _db.SaveChangesAsync();
            }

            TempData["StatusMessage"] = "✅ Lesson created.";
            return RedirectToAction(nameof(Manage), new { courseId });
        }

        [HttpGet("{courseId:int}/edit/{id:int}")]
        public async Task<IActionResult> Edit(int courseId, int id)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Attachments)
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
                SubjectId = lesson.SubjectId,
                ExistingImages = _fileUploadService.GetAttachmentDisplayVMs(lesson.Images.ToList()),
                ExistingDocuments = _fileUploadService.GetAttachmentDisplayVMs(lesson.Docs.ToList())
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
                // repopulate dropdown and existing attachments
                ViewBag.Subjects = await _db.Subjects
                    .Where(s => s.CourseId == courseId && s.IsPublished)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Title })
                    .ToListAsync();

                var lesson = await _db.Lessons
                    .Include(l => l.Attachments)
                    .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);

                if (lesson != null)
                {
                    vm.ExistingImages = _fileUploadService.GetAttachmentDisplayVMs(lesson.Images.ToList());
                    vm.ExistingDocuments = _fileUploadService.GetAttachmentDisplayVMs(lesson.Docs.ToList());
                }
                else
                {
                    vm.ExistingImages ??= new List<AttachmentDisplayVM>();
                    vm.ExistingDocuments ??= new List<AttachmentDisplayVM>();
                }

                return View("Edit", vm);
            }

            var lessonToUpdate = await _db.Lessons
                .Include(l => l.Attachments)
                .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
            if (lessonToUpdate == null) return NotFound();

            lessonToUpdate.Title = vm.Title;
            lessonToUpdate.YoutubeId = ytId;
            lessonToUpdate.YoutubeUrlRaw = vm.YoutubeUrlOrId;
            lessonToUpdate.SortOrder = vm.SortOrder;
            lessonToUpdate.IsPublished = vm.IsPublished;
            lessonToUpdate.SubjectId = vm.SubjectId;

            // Handle new file uploads
            var imageAttachments = await _fileUploadService.SaveFilesAsync(vm.LessonImages, lessonToUpdate.Id, AttachmentType.Image);
            var docAttachments = await _fileUploadService.SaveFilesAsync(vm.LessonDocuments, lessonToUpdate.Id, AttachmentType.Document);

            if (imageAttachments.Any() || docAttachments.Any())
            {
                _db.LessonAttachment.AddRange(imageAttachments);
                _db.LessonAttachment.AddRange(docAttachments);
            }

            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "✏️ Lesson updated.";
            return RedirectToAction(nameof(Manage), new { courseId });
        }

        [HttpPost("delete-attachment/{attachmentId:int}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _db.LessonAttachment
                .Include(a => a.CourseLesson)
                .FirstOrDefaultAsync(a => a.Id == attachmentId);

            if (attachment == null) return NotFound();

            var courseId = attachment.CourseLesson.CourseId;

            // Delete physical file
            _fileUploadService.DeleteFile(attachment.RelativePath);

            // Delete from database
            _db.LessonAttachment.Remove(attachment);
            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = "🗑️ Attachment deleted successfully.";
            return RedirectToAction(nameof(Edit), new { courseId, id = attachment.CourseLessonId });
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
                lessons.First(x => x.Id == dto.Ids[i]).SortOrder = i; 

            await _db.SaveChangesAsync();
            return Ok(new { updated = lessons.Count });
        }

    }
}
