using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOCS.ViewComponents
{
    public class StudentInProgressMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;

        public StudentInProgressMenuViewComponent(AppDbContext db, UserManager<Users> userManager)
        {
            _db = db; _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
                return Content(string.Empty);

            var userId = _userManager.GetUserId(HttpContext.User);

            var count = await _db.Enrollments.AsNoTracking()
                .CountAsync(e => e.StudentId == userId && e.IsApproved && !e.IsArchived);

            return View(new StudentNavVM { InProgressCount = count });
        }
    }
}