namespace SmartReportLog.Model.Users
{
    public record UserListItemDto(
        string Id,
        string UserName,
        string? FullName,
        string Role,
        bool IsLockedOut,
        DateTime CreatedAt);
}