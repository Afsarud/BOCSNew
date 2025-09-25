using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
[Route("admin/enrollments")]
public class AdminEnrollmentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<Users> _userManager;
    public AdminEnrollmentsController(AppDbContext db, UserManager<Users> userManager)
    {
        _db = db; _userManager = userManager;
    }

    // GET /admin/enrollments?status=pending|approved|archived
    [HttpGet("")]
    public async Task<IActionResult> Index(string status = "pending")
    {
        var q = _db.Enrollments.AsNoTracking();

        status = (status ?? "pending").ToLowerInvariant();
        if (status == "pending")
            q = q.Where(e => !e.IsApproved && !e.IsArchived);
        else if (status == "approved")
            q = q.Where(e => e.IsApproved && !e.IsArchived);
        else if (status == "archived")
            q = q.Where(e => e.IsArchived);

        var items =
            await (from e in q
                   join u in _db.Users.AsNoTracking()
                        on e.StudentId equals u.Id into gj
                   from u in gj.DefaultIfEmpty()   // LEFT JOIN
                   orderby e.CreatedAt descending
                   select new EnrollmentAdminItemVM
                   {
                       Id = e.Id,
                       CourseId = e.CourseId,
                       CourseTitle = e.Course.Title,
                       StudentId = e.StudentId,
                       StudentName = (u.FullName ?? u.UserName ?? u.Email) ?? "(unknown)",
                       StudentEmail = u.Email ?? "",
                       PaymentMethod = e.PaymentMethod.ToString(),
                       TransactionId = e.TransactionId,
                       SenderNumber = e.SenderNumber,
                       MobileNumber = e.MobileNumber,
                       Price = e.PriceAtEnrollment,
                       CreatedAt = e.CreatedAt,
                       IsApproved = e.IsApproved,
                       IsArchived = e.IsArchived,
                       Tic = e.Tic  // 🔹 New field bind
                   }).ToListAsync();

        ViewBag.Status = status;
        return View(items);
    }

    // POST /admin/enrollments/{id}/remove
    [HttpPost("{id:int}/remove"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, string? status)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();

        _db.Enrollments.Remove(e);           // HARD DELETE
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "🗑️ Enrollment removed.";
        return RedirectToAction(nameof(Index), new { status = status ?? "pending" });
    }

    [HttpPost("{id:int}/approve"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, bool tic, string? status)
    {
        
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id && !x.IsArchived);
        if (e == null) return NotFound();

        //if (!tic) // যদি tic=false আসে
        //{
        //    TempData["StatusMessage"] = "⚠️ You must set Tic before approving.";
        //    return RedirectToAction(nameof(Index), new { status = status ?? "pending" });
        //}

        e.Tic = tic;    // এখানে true save হবে যখন টিক দেওয়া থাকবে
        e.IsApproved = true;

        // সব lesson unlock করো
        var lessons = await _db.Lessons.Where(l => l.CourseId == e.CourseId).ToListAsync();
        foreach (var lesson in lessons)
            lesson.IsPlay = true;

        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "✅ Enrollment approved and lessons unlocked.";
        return RedirectToAction(nameof(Index), new { status = "pending" });
    }


    // POST /admin/enrollments/{id}/archive
    [HttpPost("{id:int}/archive"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();

        e.IsArchived = true;
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "🗂️ Enrollment archived.";
        return RedirectToAction(nameof(Index));
    }
}
