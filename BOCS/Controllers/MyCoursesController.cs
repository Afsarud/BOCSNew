using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOCS.Controllers
{
    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;

        public MyCoursesController(AppDbContext db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ✅ In-progress (active) courses — EndDate > Now
        [HttpGet]
        public async Task<IActionResult> InProgressIndex()
        {
            var userId = _userManager.GetUserId(User);

            // সব enrollment আনো
            var allEnrollments = await _db.Enrollments
                .Include(e => e.Course)
                .AsNoTracking()
                .Where(e => e.StudentId == userId && e.IsApproved)
                .ToListAsync();

            var inProgressItems = allEnrollments
                .Where(e => e.EndDate > DateTime.Now && !e.IsArchived)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new MyCourseItemVM
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    PriceBdt = e.Course.PriceBdt,
                    CreatedAt = e.CreatedAt,
                    EndDate = e.EndDate,
                    IsApproved = e.IsApproved,
                    IsArchived = false
                })
                .ToList();

            // Count calculate
            ViewBag.InProgressCount = inProgressItems.Count;
            ViewBag.CompletedCount = allEnrollments.Count(e => e.EndDate <= DateTime.Now);

            return View(inProgressItems); // Views/MyCourses/InProgressIndex.cshtml
        }

        // ✅ Completed (expired) courses — EndDate ≤ Now
        [HttpGet]
        public async Task<IActionResult> CompletedIndex()
        {
            var userId = _userManager.GetUserId(User);

            var allEnrollments = await _db.Enrollments
                .Include(e => e.Course)
                .AsNoTracking()
                .Where(e => e.StudentId == userId && e.IsApproved)
                .ToListAsync();

            var completedItems = allEnrollments
                .Where(e => e.EndDate <= DateTime.Now)
                .OrderByDescending(e => e.EndDate)
                .Select(e => new MyCourseItemVM
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    PriceBdt = e.Course.PriceBdt,
                    CreatedAt = e.CreatedAt,
                    EndDate = e.EndDate,
                    IsApproved = e.IsApproved,
                    IsArchived = true
                })
                .ToList();

            ViewBag.InProgressCount = allEnrollments.Count(e => e.EndDate > DateTime.Now && !e.IsArchived);
            ViewBag.CompletedCount = completedItems.Count;

            return View(completedItems); // Views/MyCourses/CompletedIndex.cshtml
        }
    }
}