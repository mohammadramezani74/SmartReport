using System.Security.Claims;
using SmartReportLog.Entity.Identity;

namespace SmartReportLog.Services.Auth
{
    /// <summary>محدوده دسترسی کاربر جاری.</summary>
    public sealed record UserScope(int? StateCode, string? StateName)
    {
        /// <summary>دسترسی سراسری — بدون محدودیت استانی.</summary>
        public static readonly UserScope Unrestricted = new(null, null);

        public bool IsRestricted => StateCode.HasValue;
    }

    public interface IUserScopeService
    {
        /// <summary>محدوده کاربر جاری را برمی‌گرداند.</summary>
        UserScope GetScope();
    }

    public sealed class UserScopeService : IUserScopeService
    {
        private readonly IHttpContextAccessor _http;

        public UserScopeService(IHttpContextAccessor http) => _http = http;

        public UserScope GetScope()
        {
            var user = _http.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return UserScope.Unrestricted;

            // مدیر سیستم همیشه به همه استان‌ها دسترسی دارد
            if (user.IsInRole(AppRoles.Admin))
                return UserScope.Unrestricted;

            var raw = user.FindFirst(AppClaims.StateCode)?.Value;

            return int.TryParse(raw, out var code)
                ? new UserScope(code, user.FindFirst(AppClaims.StateName)?.Value)
                : UserScope.Unrestricted;
        }
    }
}