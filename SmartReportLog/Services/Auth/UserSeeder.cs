using Microsoft.AspNetCore.Identity;
using SmartReportLog.Entity.Identity;

namespace SmartReportLog.Services.Auth
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            foreach (var role in AppRoles.All)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            var userName = config["Seed:AdminUserName"];
            var password = config["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return;   // اگر تنظیم نشده باشد، ادمینی ساخته نمی‌شود

            if (await userManager.FindByNameAsync(userName) is not null)
                return;

            var admin = new AppUser
            {
                UserName = userName,
                FullName = "مدیر سیستم",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
