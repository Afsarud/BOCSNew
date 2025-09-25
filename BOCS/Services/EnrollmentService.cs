using System.Security.Claims;
using System.Linq;
using BOCS.Models;
using BOCS.Data;

namespace BOCS.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly AppDbContext _db;
        public EnrollmentService(AppDbContext db) => _db = db;

        public bool HasAnyCourses(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;

            return _db.Enrollments.Any(e =>
                e.StudentId == userId &&
                e.IsApproved &&
                !e.IsArchived &&
                e.Course.IsActive);
        }
    }
}