using System.Security.Claims;
using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BOCS.Controllers
{
    // Admin-only dashboard for managing enrollments
    [Authorize(Roles = "Admin")]
    public class CourseEnrollmentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;

        public CourseEnrollmentController(AppDbContext db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ---------- LIST ----------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _db.Courses
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new CoursePickVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    DurationDays = c.DurationDays,
                    PriceBdt = c.PriceBdt,
                    ThumbnailUrl = c.ThumbnailUrl,
                    CourseType = c.CourseType
                })
                .ToListAsync();

            return View(items); // 👈 Index.cshtml expects IEnumerable<CoursePickVM>
        }

        [HttpGet]
        public async Task<IActionResult> Enroll(int id)
        {
            var c = await _db.Courses.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Title, x.PriceBdt, x.DurationDays })
                .FirstOrDefaultAsync();
            if (c == null) return NotFound();

            var start = TodayBd();
            var end = start.AddDays(Math.Max(c.DurationDays, 1) - 1);

            // শুধুমাত্র Student রোলধারী ইউজারদের তালিকা
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var options = students
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{(string.IsNullOrWhiteSpace(u.FullName) ? u.UserName : u.FullName)} ({u.Email})"
                })
                .ToList();

            var vm = new AdminEnrollmentCreateVM
            {
                CourseId = c.Id,
                CourseTitle = c.Title,
                CoursePriceBdt = c.PriceBdt,
                CourseDurationDays = c.DurationDays,
                StartDate = start,
                EndDate = end,
                StudentOptions = options
            };

            return View(vm);
        }

        // ---------- ENROLL (POST) ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(AdminEnrollmentCreateVM vm)
        {
            // ড্রপডাউন আবার ভরতে হবে (ভ্যালিডেশন ফেল করলে)
            async Task RefillStudentsAsync()
            {
                var students = await _userManager.GetUsersInRoleAsync("Student");
                vm.StudentOptions = students.OrderBy(u => u.UserName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = $"{(string.IsNullOrWhiteSpace(u.FullName) ? u.UserName : u.FullName)} ({u.Email})"
                    }).ToList();
            }

            if (string.IsNullOrWhiteSpace(vm.SelectedStudentId))
                ModelState.AddModelError(nameof(vm.SelectedStudentId), "Please choose a student.");

            if (!ModelState.IsValid)
            {
                await RefillStudentsAsync();
                return View(vm);
            }

            var course = await _db.Courses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == vm.CourseId);
            if (course == null) return NotFound();

            // তারিখ সার্ভার-সাইডে হিসাব
            var start = (vm.StartDate == default ? TodayBd() : vm.StartDate.Date);
            var end = start.AddDays(Math.Max(course.DurationDays, 1) - 1);

            var enroll = new CourseEnrollment
            {
                CourseId = vm.CourseId,
                StudentId = vm.SelectedStudentId,
                CreatedAt = DateTime.UtcNow,
                AccessType = vm.Access,
                PaymentMethod = vm.PaymentMethod,
                TransactionId = vm.TransactionId,
                SenderNumber = vm.SenderNumber,
                MobileNumber = vm.MobileNumber,
                PriceAtEnrollment = course.PriceBdt,
                StartDate = start,
                EndDate = end,
                IsApproved = false,
                IsArchived = false
            };

            _db.Enrollments.Add(enroll);
            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = "✅ Enrollment created (Pending).";
            return RedirectToAction(nameof(Index), new { status = "pending" });
        }

        // ---------- INFO ----------
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var e = await _db.Enrollments
                .Include(x => x.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (e == null) return NotFound();

            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == e.StudentId);

            var vm = new EnrollmentAdminItemVM
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                StudentId = e.StudentId,
                StudentName = u?.FullName ?? u?.UserName ?? "",
                StudentEmail = u?.Email ?? "",
                PaymentMethod = e.PaymentMethod.ToString(),
                TransactionId = e.TransactionId,
                SenderNumber = e.SenderNumber,
                MobileNumber = e.MobileNumber,
                Price = e.PriceAtEnrollment,
                CreatedAt = e.CreatedAt,
                CourseDurationDays = e.Course.DurationDays,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsApproved = e.IsApproved,
                IsArchived = e.IsArchived
            };

            return View(vm);
        }

        // ---------- ACTIONS ----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? status)
        {
            var e = await _db.Enrollments.FindAsync(id);
            if (e == null) return NotFound();

            e.IsApproved = true;
            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "✅ Approved.";
            return RedirectToAction(nameof(Index), new { status = status ?? "approved" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id, string? status)
        {
            var e = await _db.Enrollments.FindAsync(id);
            if (e == null) return NotFound();

            e.IsArchived = true;
            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "✔ Archived.";
            return RedirectToAction(nameof(Index), new { status = status ?? "archived" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, string? status)
        {
            var e = await _db.Enrollments.FindAsync(id);
            if (e == null) return NotFound();

            _db.Enrollments.Remove(e);
            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "🗑 Removed.";
            return RedirectToAction(nameof(Index), new { status = status ?? "pending" });
        }

        // Helper: BD local date
        private static DateTime TodayBd()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            }
            catch { return DateTime.Today; }
        }
    }
    public class CoursePickVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int DurationDays { get; set; }
        public decimal PriceBdt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseType CourseType { get; set; } // 1=Full, 2=Half
    }
}