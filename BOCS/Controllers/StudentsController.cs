using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using BOCS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BOCS.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;
        public StudentsController(AppDbContext db, UserManager<Users> userManager)
        { _db = db; _userManager = userManager; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var list = await _db.Enrollments.AsNoTracking()
                .Where(e => e.StudentId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new StudentEnrollmentItemVM
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    ThumbnailUrl = e.Course.ThumbnailUrl,
                    PriceBdt = e.Course.PriceBdt,
                    IsApproved = e.IsApproved,
                    IsArchived = e.IsArchived,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return View(list);   // Views/Students/Index.cshtml
        }

    }
}
