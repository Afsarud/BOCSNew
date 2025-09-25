using BOCS.Data;
using BOCS.Models;
using Microsoft.AspNetCore.Identity;

namespace BOCS.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<Users>>();
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            await EnsureRoleExists(roleManager, "Admin");
            await EnsureRoleExists(roleManager, "Teacher");
            await EnsureRoleExists(roleManager, "Student");

            // ===== Seed Admin =====
            const string adminEmail = "afsar6308@gmail.com";
            const string adminPass = "Af123456789#";

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing == null)
            {
                var adminUser = new Users
                {
                    FullName = "Afsar",
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true,
                    Role = "Admin",                // ✅ Role
                    CreatedDate = DateTime.Now     // ✅ CreatedDate
                };

                var result = await userManager.CreateAsync(adminUser, adminPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }

            // ===== Seed Teacher =====
            const string teacherEmail = "teacher@example.com";
            const string teacherPass = "Teacher@123";

            var teacher = await userManager.FindByEmailAsync(teacherEmail);
            if (teacher == null)
            {
                var teacherUser = new Users
                {
                    FullName = "Sobuz",
                    Email = teacherEmail,
                    UserName = teacherEmail,
                    EmailConfirmed = true,
                    Role = "Teacher",             // ✅ Role যোগ করা হলো
                    CreatedDate = DateTime.Now    // ✅ CreatedDate যোগ করা হলো
                };

                var result = await userManager.CreateAsync(teacherUser, teacherPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
                }
                else
                {
                    throw new Exception("Teacher create failed: "
                        + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // ===== Seed Student =====
            const string studentEmail = "student@example.com";
            const string studentPass = "Student@123";

            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                var studentUser = new Users
                {
                    FullName = "Sumaia",
                    Email = studentEmail,
                    UserName = studentEmail,
                    EmailConfirmed = true,
                    Role = "Student",             // ✅ Role যোগ করা হলো
                    CreatedDate = DateTime.Now    // ✅ CreatedDate যোগ করা হলো
                };

                var result = await userManager.CreateAsync(studentUser, studentPass);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
                else
                {
                    throw new Exception("Student create failed: "
                        + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task EnsureRoleExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}

