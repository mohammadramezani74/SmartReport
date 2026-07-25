namespace SmartReportLog.Model.Ticket
{
    public record AtmTicketDetailDto(
        string? TicketNumber,      // آنچه دستگاه فرستاده — ممکن است تأیید نشده باشد
        string? PersonnelCode,
        int? RequestNo,            // آنچه ERDB تأیید کرده
        string? CallDate,
        string? ReferDate,
        string? ReferEndTime,
        string? AssignType,
        string? TechName,
        DateTime? FetchedAt)
    {
        public bool IsVerified => RequestNo.HasValue;
    }
}

