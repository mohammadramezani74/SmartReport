namespace SmartReportLog.Model.Ticket
{
    public record AtmLocationDto(
       string? MInvCode, string? DeviceName, int? StateCode, string? StateName,
       string? CityName, string? SupervisionStateName, string? CustomerName,
       string? BranchCode, string? BranchName);
}

