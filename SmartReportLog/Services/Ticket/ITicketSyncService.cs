using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Ticket;

namespace SmartReportLog.Services.Ticket
{
    public interface ITicketSyncService
    {
        Task<TicketSyncResult> SyncAsync(string? serialNumber, string? ticketNumber, CancellationToken ct);

        Task<string?> AttachAsync(Atm atm, IReadOnlyCollection<AtmDailyAnalysis> reports,
            string? ticketNumber, CancellationToken ct);
    }
}
