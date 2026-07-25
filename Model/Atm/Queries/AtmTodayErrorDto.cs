namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmTodayErrorDto(string Device, string ErrorCode, int Count, DateOnly Date);
}
