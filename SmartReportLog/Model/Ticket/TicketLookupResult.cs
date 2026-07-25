namespace SmartReportLog.Model.Ticket
{
    public sealed record TicketLookupResult(bool Success, string? Message, TicketInfoRow? Data);

}

