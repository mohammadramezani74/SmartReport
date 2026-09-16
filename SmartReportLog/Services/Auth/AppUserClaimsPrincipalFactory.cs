using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SmartReportLog.Entity.Identity;

namespace SmartReportLog.Services.Auth
{
    /// <summary>
    /// کد استان و نام کامل کاربر را به صورت claim به کوکی احراز هویت اضافه می‌کند
    /// تا در هر درخواست نیازی به مراجعه به دیتابیس نباشد.
    /// </summary>
    public sealed class AppUserClaimsPrincipalFactory
        : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrWhiteSpace(user.FullName))
                identity.AddClaim(new Claim(AppClaims.FullName, user.FullName));

            if (user.StateCode.HasValue)
            {
                identity.AddClaim(new Claim(
                    AppClaims.StateCode,
                    user.StateCode.Value.ToString()));

                if (!string.IsNullOrWhiteSpace(user.StateName))
                    identity.AddClaim(new Claim(AppClaims.StateName, user.StateName));
            }

            return identity;
        }
    }
}