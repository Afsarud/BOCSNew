using BOCS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BOCS.Controllers
{
    public class SessionTestController : Controller
    {
        private readonly UserManager<Users> _userManager;

        public SessionTestController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionIdClaim = User.FindFirstValue("SessionId");

            var user = await _userManager.FindByIdAsync(userId);

            ViewBag.UserId = userId;
            ViewBag.SessionIdClaim = sessionIdClaim;
            ViewBag.DatabaseSessionId = user?.CurrentSessionId;
            ViewBag.SessionMatch = sessionIdClaim == user?.CurrentSessionId;

            return View();
        }
    }
}
