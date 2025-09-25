using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Teacher")]
[Route("[controller]")]
public class CourseSubjectsController : Controller
{
    private readonly AppDbContext _db;
    public CourseSubjectsController(AppDbContext db) => _db = db;

    // GET /admin/course-subjects  -> কোর্স তালিকা (manage subjects)
    [HttpGet("")]
    public async Task<IActionResult> Courses()
    {
        var list = await _db.Courses.AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new CourseMiniVM
            {
                Id = c.Id,
                Title = c.Title,
                CourseType= c.CourseType,
                SubjectCount = _db.Subjects.Count(s => s.CourseId == c.Id)
            }).ToListAsync();

        return View("Courses", list); // Views/CourseSubjects/Courses.cshtml
    }

    // GET /admin/course-subjects/{courseId}
    [HttpGet("{courseId:int}")]
    public async Task<IActionResult> Index(int courseId)
    {
        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        var vm = new SubjectManageVM
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            Subjects = await _db.Subjects.AsNoTracking()
                        .Where(s => s.CourseId == courseId)
                        .OrderBy(s => s.SortOrder)
                        .Select(s => new SubjectItemVM
                        {
                            Id = s.Id,
                            Title = s.Title,
                            SortOrder = s.SortOrder,
                            IsPublished = s.IsPublished,
                            LessonCount = _db.Lessons.Count(l => l.SubjectId == s.Id)
                        }).ToListAsync()
        };
        return View(vm); // Views/CourseSubjects/Index.cshtml
    }

    // GET /admin/course-subjects/{courseId}/create
    [HttpGet("{courseId:int}/create")]
    public IActionResult Create(int courseId) =>
        View(new SubjectCreateVM { CourseId = courseId });

    // POST /admin/course-subjects/{courseId}/create
    [HttpPost("{courseId:int}/create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, SubjectCreateVM vm)
    {
        if (courseId != vm.CourseId) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        _db.Subjects.Add(new CourseSubject
        {
            CourseId = vm.CourseId,
            Title = vm.Title,
            SortOrder = vm.SortOrder,
            IsPublished = vm.IsPublished
        });
        await _db.SaveChangesAsync();
        TempData["StatusMessage"] = "✅ Subject created.";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    // GET /admin/course-subjects/{courseId}/delete/{id}
    [HttpGet("{courseId:int}/delete/{id:int}")]
    public async Task<IActionResult> Delete(int courseId, int id)
    {
        var s = await _db.Subjects.AsNoTracking()
                  .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
        if (s == null) return NotFound();

        return View(new SubjectDeleteVM
        {
            CourseId = courseId,
            Id = id,
            Title = s.Title
        });
    }

    // GET /admin/course-subjects/{courseId}/edit/{id}
    [HttpGet("{courseId:int}/edit/{id:int}")]
    public async Task<IActionResult> Edit(int courseId, int id)
    {
        var s = await _db.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
        if (s == null) return NotFound();

        var vm = new SubjectEditVM
        {
            CourseId = s.CourseId,
            Id = s.Id,
            Title = s.Title,
            SortOrder = s.SortOrder,
            IsPublished = s.IsPublished
        };
        return View(vm); // Views/CourseSubjects/Edit.cshtml
    }

    // POST /admin/course-subjects/{courseId}/edit/{id}
    [HttpPost("{courseId:int}/edit/{id:int}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int courseId, int id, SubjectEditVM vm)
    {
        if (courseId != vm.CourseId || id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var s = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
        if (s == null) return NotFound();

        s.Title = vm.Title;
        s.SortOrder = vm.SortOrder;
        s.IsPublished = vm.IsPublished;

        await _db.SaveChangesAsync();
        TempData["StatusMessage"] = "✏️ Subject updated.";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    // POST /admin/course-subjects/{courseId}/delete/{id}
    [HttpPost("{courseId:int}/delete/{id:int}"), ValidateAntiForgeryToken, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int courseId, int id)
    {
        var s = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId);
        if (s == null) return NotFound();

        // Subject ডিলিট হলে ঐ Subject-এর Lesson গুলো SubjectId=null হবে (DbContext config অনুযায়ী)
        _db.Subjects.Remove(s);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "🗑️ Subject deleted.";
        return RedirectToAction(nameof(Index), new { courseId });
    }
}