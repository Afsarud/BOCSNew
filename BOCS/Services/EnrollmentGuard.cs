using Microsoft.EntityFrameworkCore;
using BOCS.Data;

namespace BOCS.Services
{
    public static class EnrollmentGuard
    {
        public static DateTime TodayBd()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        }

        public static Task<bool> HasCourseAccessAsync(this AppDbContext db, string? userId, int courseId)
        {
            if (string.IsNullOrEmpty(userId)) return Task.FromResult(false);
            var today = TodayBd();

            return db.Enrollments.AnyAsync(e =>
                e.CourseId == courseId &&
                e.StudentId == userId &&
                e.IsApproved && !e.IsArchived &&
                e.StartDate <= today && e.EndDate >= today);
        }
    }
}
