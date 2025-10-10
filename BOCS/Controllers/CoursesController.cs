using BOCS.Data;
using BOCS.Models;
using BOCS.ModelsView;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BOCS.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UserManager<Users> _userManager; 
        private readonly AppDbContext _db;
        public CoursesController(AppDbContext db, UserManager<Users> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task<IActionResult> CourseIndex(string? q, string? sort = "recent")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _db.Courses
                .Where(c => c.IsActive)
                .Select(c => new CourseListItemVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Summary = "",        
                    TeacherName = null,       
                    ThumbnailUrl = c.ThumbnailUrl,    
                    IsActive = c.IsActive,
                    IsNew = false,   
                    IsEnrolled = userId != null && _db.Enrollments
                        .Any(e => e.StudentId == userId && e.CourseId == c.Id && e.IsApproved && !e.IsArchived)
                });

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.Title.Contains(q));

            query = sort switch
            {
                "title" => query.OrderBy(x => x.Title),
                "popular" => query.OrderByDescending(x => x.IsEnrolled),
                _ => query.OrderByDescending(x => x.IsNew).ThenBy(x => x.Title)
            };

            var list = await query.AsNoTracking().ToListAsync();
            ViewData["q"] = q;
            ViewData["sort"] = sort;
            return View("CourseIndex", list);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var vm = new CourseListItemVM
            {
                Id = course.Id,
                Title = course.Title,
                
                Summary = "", 
                IsActive = course.IsActive
            };

            return View(vm);
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _db.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c => c.Title.Contains(q));
            // কেবল Active course
            query = query.Where(c => c.IsActive);

            var list = await query
                .OrderBy(c => c.Id)
                .Select(c => new CourseCatalogItemVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    ThumbnailUrl = c.ThumbnailUrl,
                    DurationDays = c.DurationDays,
                    PriceBdt = c.PriceBdt,
                    CourseType = c.CourseType,
                    NotificationCount = 0
                })
                .ToListAsync();

            ViewBag.Query = q;
            return View(list);
        }

        public async Task<IActionResult> Info(int id)
        {
            var course = await _db.Courses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            var lessons = await _db.Lessons.AsNoTracking()
                .Where(l => l.CourseId == id && l.IsPublished)
                .OrderBy(l => l.SortOrder)
                .Select(l => new { l.Id, l.Title, l.YoutubeId, l.SubjectId, l.IsPlay })
                .ToListAsync();

            var subjects = await _db.Subjects.AsNoTracking()
                .Where(s => s.CourseId == id && s.IsPublished)
                .OrderBy(s => s.SortOrder)
                .Select(s => new { s.Id, s.Title })
                .ToListAsync();

            var outlines = new List<OutlineGroupVM>();
            foreach (var s in subjects)
            {
                outlines.Add(new OutlineGroupVM
                {
                    Title = s.Title,
                    Items = lessons.Where(x => x.SubjectId == s.Id)
                                   .Select(x => new OutlineItemVM { Label = x.Title, YoutubeId = x.YoutubeId })
                                   .ToList()
                });
            }
            var orphans = lessons.Where(x => x.SubjectId == null).ToList();
            if (orphans.Count > 0)
            {
                outlines.Add(new OutlineGroupVM
                {
                    Title = "Other lessons",
                    Items = orphans.Select(x => new OutlineItemVM { Label = x.Title, YoutubeId = x.YoutubeId }).ToList()
                });
            }

            // ytId => IsPlay map (ভিউতে data-attribute বানাতে)
            ViewBag.LessonPlay = lessons
                .GroupBy(x => x.YoutubeId)
                .ToDictionary(g => g.Key, g => g.Any(z => z.IsPlay));

            // প্রথম play-able yt id (থাকলে)
            string? firstPlayableId = lessons
                .Where(x => x.IsPlay)
                .Select(x => x.YoutubeId)
                .FirstOrDefault();

            return View(new CourseInfoVM
            {
                Id = course.Id,
                Title = course.Title,
                ThumbnailUrl = course.ThumbnailUrl,
                DurationDays = course.DurationDays,
                PriceBdt = course.PriceBdt,
                CreatedBy = "Admin",
                Outlines = outlines,
                // ভিউতে initialId হিসেবে আমরা আলাদা স্ক্রিপ্টে দেব
                LatestYoutubeId = firstPlayableId
            });
        }

        [HttpGet]
        public async Task<IActionResult> Enroll(int id)
        {
            var course = await _db.Courses
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new { c.Id, c.Title, c.PriceBdt, c.DurationDays })
                .FirstOrDefaultAsync();

            if (course == null) return NotFound();

            var start = TodayBd();
            var end = start.AddDays(Math.Max(course.DurationDays, 1) - 1); // inclusive

            var vm = new EnrollmentCreateVM
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                CoursePriceBdt = course.PriceBdt,
                CourseDurationDays = course.DurationDays,
                StartDate = start,
                EndDate = end
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollmentCreateVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var course = await _db.Courses.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == vm.CourseId);
            if (course == null) return NotFound();

            // trust user's StartDate (from date picker), but compute EndDate here
            var start = vm.StartDate.Date;
            var end = start.AddDays(Math.Max(course.DurationDays, 1) - 1);

            var enroll = new CourseEnrollment
            {
                CourseId = vm.CourseId,
                StudentId = _userManager.GetUserId(User)!,
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
            return RedirectToAction(nameof(Index), new { id = vm.CourseId  });
        }

        private static DateTime TodayBd()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }

    }
}
