using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOCS.Controllers
{
    public class MyCoursesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;
        public MyCoursesController(AppDbContext db, UserManager<Users> userManager)
        { _db = db; _userManager = userManager; }

        [HttpGet]
        public async Task<IActionResult> InProgressIndex()
        {
            var userId = _userManager.GetUserId(User);

            var items = await _db.Enrollments.AsNoTracking()
                .Where(e => e.StudentId == userId && e.IsApproved && !e.IsArchived)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new MyCourseItemVM
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    PriceBdt = e.Course.PriceBdt
                })
                .ToListAsync();

            return View(items); // Views/MyCourses/InProgressIndex.cshtml
        }
        public IActionResult CompletedIndex()
        {
            return View();
        }
    }
}
