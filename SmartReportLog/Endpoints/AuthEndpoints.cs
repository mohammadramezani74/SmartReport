using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartReportLog.Entity.Identity;

namespace SmartReportLog.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/login", async (
                HttpContext http,
                [FromForm] string userName,
                [FromForm] string password,
                [FromForm] bool rememberMe,
                [FromForm] string? returnUrl,
                SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager) =>
            {
                var user = await userManager.FindByNameAsync(userName ?? string.Empty);

                if (user is null)
                    return Results.Redirect(Fail("invalid", returnUrl));

                var result = await signInManager.PasswordSignInAsync(
                    user, password ?? string.Empty, rememberMe, lockoutOnFailure: true);

                if (result.IsLockedOut)
                    return Results.Redirect(Fail("locked", returnUrl));

                if (!result.Succeeded)
                    return Results.Redirect(Fail("invalid", returnUrl));

                return Results.Redirect(SafeReturn(returnUrl));
            })
            .AllowAnonymous()
            .DisableAntiforgery();   // AntiforgeryToken دستی در فرم ارسال می‌شود

            group.MapPost("/logout", async (SignInManager<AppUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return Results.Redirect("/login");
            });
        }

        private static string Fail(string code, string? returnUrl)
        {
            var url = $"/login?error={code}";
            return string.IsNullOrWhiteSpace(returnUrl)
                ? url
                : $"{url}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        // فقط مسیرهای داخلی — جلوگیری از open redirect
        private static string SafeReturn(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl)) return "/";

            var decoded = Uri.UnescapeDataString(returnUrl);

            return decoded.StartsWith('/') && !decoded.StartsWith("//")
                ? decoded
                : "/";
        }
    }
}