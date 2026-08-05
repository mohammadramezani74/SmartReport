using Microsoft.AspNetCore.Identity;

namespace SmartReportLog.Entity.Identity
{
    public sealed class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Viewer };
    }
}
