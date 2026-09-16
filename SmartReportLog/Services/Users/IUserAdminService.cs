using SmartReportLog.Model.Users;

namespace SmartReportLog.Services.Users
{
    public interface IUserAdminService
    {
        Task<List<UserListItemDto>> GetUsersAsync(CancellationToken ct);

        /// <summary>استان‌هایی که در سیستم دستگاه دارند، برای تخصیص به کاربر.</summary>
        Task<List<StateOptionDto>> GetStateOptionsAsync(CancellationToken ct);

        Task<UserActionResult> CreateAsync(
            string userName, string? fullName, string password,
            string role, int? stateCode, CancellationToken ct);

        Task<UserActionResult> UpdateAsync(
            string userId, string? fullName, string role,
            int? stateCode, CancellationToken ct);

        Task<UserActionResult> ResetPasswordAsync(
            string userId, string newPassword, CancellationToken ct);

        Task<UserActionResult> SetLockAsync(string userId, bool locked, CancellationToken ct);

        Task<UserActionResult> DeleteAsync(string userId, CancellationToken ct);
    }
}