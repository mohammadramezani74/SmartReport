namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmListItemDto(
     Guid Id,
     string SerialNumber,
     DateOnly? LastReportEnd,
     int TotalReportsCount,
     int? StateCode,
     string? StateName,
     string? CityName,
     string? BranchName,
     string? DeviceName);
    public record AtmDashboardSummaryDto(
    int TotalAtmsCount,
    int ActiveAtmsCount,
    int TodayTotalReportsCount,
    int TodayTotalErrorsCount
);

    public record AtmStateOptionDto(int Code, string Name, int AtmCount);
    public record AtmErrorTypeDistributionDto(
        string ErrorCategory, 
        int Count,          
        string ColorHex    
    );
}
