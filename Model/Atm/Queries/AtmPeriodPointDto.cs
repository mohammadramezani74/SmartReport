namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmPeriodPointDto(
        DateOnly Date, DateOnly EndDate, int TotalCards, int TotalTransactions, int ReceiptCount,
        int DailyDispenseTotal, int DailyRejectTotal, int? CpuTemperatureC);
}
