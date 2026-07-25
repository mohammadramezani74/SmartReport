using SmartReportLog.Model.Ticket;

namespace SmartReportLog.Services.Ticket
{
    public interface ITicketInfoProvider
    {
        Task<TicketLookupResult> GetByTicketNumberAsync(string? ticketNumber, CancellationToken ct);
    }
}
