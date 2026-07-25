using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Ticket;
using SmartReportLog.Persistance;

namespace SmartReportLog.Services.Ticket
{
    public sealed class TicketSyncService : ITicketSyncService
    {
        private readonly SmartLogContext _context;
        private readonly ITicketInfoProvider _ticketProvider;
        private readonly ILogger<TicketSyncService> _logger;

        public TicketSyncService(
            SmartLogContext context,
            ITicketInfoProvider ticketProvider,
            ILogger<TicketSyncService> logger)
        {
            _context = context;
            _ticketProvider = ticketProvider;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // فراخوانی مستقل: با سریال و شماره تیکت، اطلاعات را از ERDB می‌گیرد
        // و روی دستگاه و گزارش‌های مرتبط به‌روزرسانی می‌کند.
        // ------------------------------------------------------------------
        public async Task<TicketSyncResult> SyncAsync(
            string? serialNumber, string? ticketNumber, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                return new TicketSyncResult(false, "شماره سریال ارسال نشده است.", 0);

            if (string.IsNullOrWhiteSpace(ticketNumber))
                return new TicketSyncResult(false, "شماره تیکت ارسال نشده است.", 0);

            var serial = serialNumber.Trim();
            var ticket = ticketNumber.Trim();

            var atm = await _context.Atms
                .FirstOrDefaultAsync(a => a.SerialNumber == serial, ct);

            if (atm is null)
                return new TicketSyncResult(false, $"دستگاهی با سریال {serial} یافت نشد.", 0);

            var reports = await _context.DailyAnalyses
                .Where(d => d.AtmId == atm.Id && d.TicketNumber == ticket)
                .ToListAsync(ct);

            var warning = await AttachAsync(atm, reports, ticket, ct);

            if (warning is not null)
                return new TicketSyncResult(false, warning, 0);

            await _context.SaveChangesAsync(ct);

            var message = reports.Count == 0
                ? "اطلاعات دستگاه به‌روزرسانی شد، اما گزارشی با این شماره تیکت یافت نشد."
                : $"اطلاعات دستگاه و {reports.Count} گزارش به‌روزرسانی شد.";

            return new TicketSyncResult(true, message, reports.Count);
        }

        // ------------------------------------------------------------------
        // هسته مشترک: توسط SyncAsync و AtmIngestionService استفاده می‌شود.
        // ذخیره‌سازی انجام نمی‌دهد — فراخواننده باید SaveChanges بزند.
        // خروجی: پیام هشدار در صورت مشکل، در غیر این صورت null.
        // ------------------------------------------------------------------
        public async Task<string?> AttachAsync(
            Atm atm, IReadOnlyCollection<AtmDailyAnalysis> reports,
            string? ticketNumber, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                return null;

            var lookup = await _ticketProvider.GetByTicketNumberAsync(ticketNumber, ct);

            if (!lookup.Success || lookup.Data is null)
                return lookup.Message;

            var t = lookup.Data;

            if (!SerialsMatch(t.SerialNo, atm.SerialNumber))
            {
                _logger.LogWarning(
                    "عدم تطابق سریال. تیکت={TicketNo} سریال‌ERDB='{ErdbSerial}' سریال‌دستگاه='{DeviceSerial}'",
                    t.RequestNo, t.SerialNo, atm.SerialNumber);

                return $"سریال تیکت ({t.SerialNo}) با سریال دستگاه ({atm.SerialNumber}) مطابقت ندارد.";
            }

            atm.UpdateLocationInfo(
                t.MInvCode, t.DeviceName, t.StateCode, t.StateName,
                t.CityName, t.SupervisionStateName, t.CustomerName,
                t.BranchCode, t.BranchName);

            if (reports.Count == 0)
                return null;

            var reportIds = reports.Select(r => r.Id).ToList();

            var existingTickets = await _context.AtmTicketInfos
                .Where(x => reportIds.Contains(x.DailyAnalysisId))
                .ToListAsync(ct);

            foreach (var report in reports)
            {
                var existing = existingTickets
                    .FirstOrDefault(x => x.DailyAnalysisId == report.Id);

                if (existing is null)
                {
                    _context.AtmTicketInfos.Add(AtmTicketInfo.Create(
                        report.Id, t.RequestNo, t.CallDate, t.ReferDate,
                        t.ReferEndTime, t.AssignType, t.TechName));
                }
                else
                {
                    existing.Refresh(t.CallDate, t.ReferDate,
                        t.ReferEndTime, t.AssignType, t.TechName);
                }
            }

            return null;
        }

        private static bool SerialsMatch(string? erdbSerial, string? deviceSerial)
            => Canonical(erdbSerial) is { Length: > 0 } a
            && Canonical(deviceSerial) is { Length: > 0 } b
            && a == b;

  
        private static string Canonical(string? s)
            => string.IsNullOrWhiteSpace(s)
                ? string.Empty
                : new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}