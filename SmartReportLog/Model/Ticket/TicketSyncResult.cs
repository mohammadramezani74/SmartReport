namespace SmartReportLog.Model.Ticket
{
    public sealed record TicketSyncResult(bool Success, string? Message, int UpdatedReportsCount);

}
