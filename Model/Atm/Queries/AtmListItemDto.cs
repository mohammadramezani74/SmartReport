namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmListItemDto(Guid Id, string SerialNumber, DateOnly? LastReportEnd, int TotalReportsCount);
    public record AtmDashboardSummaryDto(
    int TotalAtmsCount,
    int ActiveAtmsCount,
    int TodayTotalReportsCount,
    int TodayTotalErrorsCount
);


    public record AtmErrorTypeDistributionDto(
        string ErrorCategory, 
        int Count,          
        string ColorHex    
    );
}
