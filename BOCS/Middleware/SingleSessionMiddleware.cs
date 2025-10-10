using BOCS.Data;
using BOCS.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BOCS.Middleware
{
    public class SingleSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SingleSessionMiddleware> _logger;

        public SingleSessionMiddleware(RequestDelegate next, ILogger<SingleSessionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<Users> userManager, SignInManager<Users> signInManager, AppDbContext dbContext)
        {
            // Skip session validation for anonymous users
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Skip session validation for login/logout pages to avoid redirect loops
            var path = context.Request.Path.Value?.ToLower();
            if (path?.Contains("/account/login") == true ||
                path?.Contains("/account/logout") == true ||
                path?.Contains("/account/register") == true)
            {
                await _next(context);
                return;
            }

            try
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var sessionIdClaim = context.User.FindFirstValue("SessionId");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(sessionIdClaim))
                {
                    _logger.LogWarning("Missing user ID or session ID claim for user {UserId}", userId);
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // Get current session ID from database
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database: {UserId}", userId);
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // Check if session ID matches
                if (user.CurrentSessionId != sessionIdClaim)
                {
                    _logger.LogInformation("Session ID mismatch for user {UserId}. Expected: {ExpectedSessionId}, Actual: {ActualSessionId}",
                        userId, user.CurrentSessionId, sessionIdClaim);
                    await SignOutAndRedirect(context, signInManager);
                    return;
                }

                // Session is valid, continue
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SingleSessionMiddleware for user {UserId}",
                    context.User.FindFirstValue(ClaimTypes.NameIdentifier));

                // On error, sign out to be safe
                await SignOutAndRedirect(context, signInManager);
            }
        }

        private async Task SignOutAndRedirect(HttpContext context, SignInManager<Users> signInManager)
        {
            await signInManager.SignOutAsync();
            context.Response.Redirect("/Account/Login?reason=session_expired");
        }
    }
}
