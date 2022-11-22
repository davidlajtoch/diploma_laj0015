using DiplomaThesis.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace DiplomaThesis.Server.Data;

public static class ApplicationDbInitializer
{
    public static void SeedUsers(UserManager<ApplicationUser> userManager)
    {
        if (userManager.FindByEmailAsync("admin@admin.cz").Result is null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@admin.cz",
                Email = "admin@admin.cz"
            };

            var result = userManager.CreateAsync(user, "admin1234").Result;

            if (result.Succeeded) userManager.AddToRolesAsync(user, new[] { "Admin", "Architect" }).Wait();
        }
    }
}