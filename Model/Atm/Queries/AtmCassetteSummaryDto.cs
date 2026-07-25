namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmCassetteSummaryDto(int CassetteIndex, long Denomination, int TotalPickup, int TotalDispense, int TotalReject);

}
