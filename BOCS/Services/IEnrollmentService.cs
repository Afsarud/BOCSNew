using System.Security.Claims;

namespace BOCS.Services
{
    public interface IEnrollmentService
    {
        bool HasAnyCourses(ClaimsPrincipal user);
    }
}