using BOCS.Data;
using BOCS.Models;
using BOCS.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ---------- Added by Afsar ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Student "My Course" menu helper
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// File upload service
builder.Services.AddScoped<FileUploadService>();

// Auth cookie (session-like)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = false;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
}
// ---------- end by Afsar ----------

var app = builder.Build();

// Seed (roles/users)
using (var scope = app.Services.CreateScope())
{
    await SeedService.SeedDatabase(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ---------- Security headers + CSP (ONE place, BEFORE static files) ----------
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
    ctx.Response.Headers["Pragma"] = "no-cache";
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";

    if (app.Environment.IsDevelopment())
    {
        ctx.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data: https://i.ytimg.com https:; " +
            "style-src 'self'; " +             
            "script-src 'self'; " +             
            "frame-src 'self' https://www.youtube.com https://www.youtube-nocookie.com; " +
            "connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*;";
    }
    else
    {
        ctx.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data: https://i.ytimg.com; " +
            "style-src 'self'; " +
            "script-src 'self'; " +
            "frame-src 'self' https://www.youtube.com https://www.youtube-nocookie.com; " +
            "connect-src 'self';";
    }

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseMiddleware<BOCS.Middleware.SingleSessionMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();