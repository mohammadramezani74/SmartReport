namespace SmartReportLog.Model.Atm.Queries
{
    public record AtmPeriodListItemDto(
    Guid Id, string StartDate, string EndDate,
    int TotalTransactions, int TotalCards, int ReceiptCount, int ErrorsCount);
}
