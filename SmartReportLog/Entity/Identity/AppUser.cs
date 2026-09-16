using Microsoft.AspNetCore.Identity;

namespace SmartReportLog.Entity.Identity
{
    public sealed class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// کد استانی که کاربر به آن محدود است.
        /// null یعنی دسترسی به همه استان‌ها (مدیر سیستم یا کاربر سراسری).
        /// </summary>
        public int? StateCode { get; set; }

        /// <summary>نام استان — فقط برای نمایش، مرجع همان StateCode است.</summary>
        public string? StateName { get; set; }
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Viewer };
    }

    public static class AppClaims
    {
        /// <summary>کد استان کاربر در توکن احراز هویت.</summary>
        public const string StateCode = "state_code";

        public const string StateName = "state_name";
        public const string FullName = "full_name";
    }
}