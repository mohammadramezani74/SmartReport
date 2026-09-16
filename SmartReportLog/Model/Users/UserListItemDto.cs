namespace SmartReportLog.Model.Users
{
    public record UserListItemDto(
        string Id,
        string UserName,
        string? FullName,
        string Role,
        int? StateCode,
        string? StateName,
        bool IsLockedOut,
        DateTime CreatedAt)
    {
        public bool IsStateScoped => StateCode.HasValue;
    }

    /// <summary>گزینه‌های دراپ‌داون استان — فقط استان‌هایی که دستگاه ثبت‌شده دارند.</summary>
    public record StateOptionDto(int Code, string Name, int AtmCount);
}