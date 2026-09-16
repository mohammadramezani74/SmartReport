using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Atm.Submit;
using SmartReportLog.Persistance;
using SmartReportLog.Services.Ticket;
using System.Text.Json;

namespace SmartReportLog.Services.atm.Command.SaveData
{
    public sealed class AtmIngestionService : IAtmIngestionService
    {
        private readonly SmartLogContext _context;
        private readonly ITicketInfoProvider _ticketProvider;
        private readonly ILogger<AtmIngestionService> _logger;

        public AtmIngestionService(
            SmartLogContext context,
            ITicketInfoProvider ticketProvider,
            ILogger<AtmIngestionService> logger)
        {
            _context = context;
            _ticketProvider = ticketProvider;
            _logger = logger;
        }

        public async Task<IngestResult> IngestAsync(AtmScanDayDto report, CancellationToken ct)
        {
            if (report == null || string.IsNullOrWhiteSpace(report.S))
                return new IngestResult(false, "داده نامعتبر است.");

            if (!DateOnly.TryParse(report.Dt.Split(',').First(), out var start) ||
                !DateOnly.TryParse(report.Dt.Split(',').Last(), out var end))
                return new IngestResult(false, "فرمت تاریخ نامعتبر است.");

            // ---------- آرشیو payload خام ----------
            var jsonDocument = await _context.jsonDocuments
                .FirstOrDefaultAsync(x => x.StartDate == start
                                       && x.EndDate == end
                                       && x.SerialNumber!.Trim().ToLower() == report.S.Trim().ToLower(), ct);

            var content = JsonSerializer.Serialize(report);

            if (jsonDocument == null)
                _context.jsonDocuments.Add(JsonDocuments.Create(start, end, content, report.S));
            else
                jsonDocument.Edit(content);

            // ---------- دستگاه ----------
            var atm = await _context.Atms
                .Include(a => a.DailyAnalyses).ThenInclude(x => x.Cassettes)
                .Include(a => a.DailyAnalyses).ThenInclude(x => x.HardwareErrors)
                .Include(a => a.DailyAnalyses).ThenInclude(x => x.TodayErrors)
                .FirstOrDefaultAsync(a => a.SerialNumber == report.S, ct);

            if (atm is null)
            {
                atm = Atm.Create(report.S);
                await _context.Atms.AddAsync(atm, ct);
            }

            atm.UpdateHardwareInfo(report.Cm, report.Os);
            int? cpuTemp = report.Ct >= 0 ? report.Ct : null;

            var parsedTodayErrors = new List<(string Device, string ErrorCode, int Count, DateOnly Date)>();
            foreach (var e in report.TodayEr)
            {
                if (DateOnly.TryParse(e.Dt, out var errDate))
                    parsedTodayErrors.Add((e.D, e.E, e.C, errDate));
            }

            // ---------- گزارش دوره ----------
            AtmDailyAnalysis analysis;

            var existingReport = await _context.DailyAnalyses
                .FirstOrDefaultAsync(d => d.Date == start && d.EndDate == end && d.AtmId == atm.Id, ct);

            if (existingReport != null)
            {
                existingReport.TotalCards = report.Ca;
                existingReport.TotalTransactions = report.Tr;
                existingReport.ReceiptCount = report.Re;
                existingReport.AuiSeconds = report.Au;
                existingReport.DailyDispenseTotal = report.Di;
                existingReport.DailyRejectTotal = report.Rj;
                existingReport.ModifiedDate = DateTime.Now;
                existingReport.CpuUsagePercent = report.Cu;
                existingReport.RamTotalGb = report.Rt;
                existingReport.RamUsedGb = report.Ru;
                existingReport.CpuTemperatureC = cpuTemp;
                existingReport.DiskTotalGb = report.Dst;
                existingReport.DiskUsedGb = report.Du;
                existingReport.GayaVersion = report.Ver;
                existingReport.ImageVersion = report.Iv;
                existingReport.TicketNumber = report.Tk;
                existingReport.PersonnelCode = report.Pc;

                _context.RemoveRange(existingReport.Cassettes);
                _context.RemoveRange(existingReport.HardwareErrors);

                foreach (var item in report.Cs)
                    _context.DailyCassettes.Add(AtmCassetteDaily.Create(
                        existingReport.Id, item.I, item.Dn, item.Pk, item.Dp, item.Rj, item.Lk));

                foreach (var e in report.Er)
                    _context.DailyHardwareErrors.Add(AtmHardwareErrorDaily.Create(
                        existingReport.Id, e.D, e.E, e.C));

                var existingKeys = existingReport.TodayErrors
                    .Select(t => (t.Device, t.ErrorCode, t.Date))
                    .ToHashSet();

                foreach (var t in parsedTodayErrors)
                {
                    var key = (t.Device, t.ErrorCode, t.Date);
                    if (existingKeys.Contains(key)) continue;

                    _context.AtmTodayErrors.Add(AtmTodayError.Create(
                        existingReport.Id, t.Device, t.ErrorCode, t.Count, t.Date));
                    existingKeys.Add(key);
                }

                analysis = existingReport;
            }
            else
            {
                analysis = atm.UpsertDailyAnalysis(
                    start, end, report.Ca, report.Tr, report.Re, report.Au, report.Di, report.Rj,
                    report.Cs.Select(c => (c.I, c.Dn, c.Pk, c.Dp, c.Rj, c.Lk)),
                    report.Er.Select(e => (e.D, e.E, e.C)),
                    parsedTodayErrors,
                    report.Cu, report.Rt, report.Ru, cpuTemp, report.Dst, report.Du, report.Ver, report.Iv,
                    report.Tk, report.Pc);

                _context.DailyAnalyses.Add(analysis);
            }

            // ---------- اطلاعات تیکت از ERDB ----------
            var warning = await TryAttachTicketInfoAsync(report, atm, analysis, ct);

            await _context.SaveChangesAsync(ct);

            return new IngestResult(
                true,
                $"گزارش {start} تا {end} برای دستگاه {report.S} ثبت شد.",
                warning);
        }

        /// <summary>
        /// اطلاعات تیکت را از ERDB می‌گیرد و در صورت معتبر بودن به گزارش وصل می‌کند.
        /// در صورت بروز مشکل، ثبت گزارش متوقف نمی‌شود و فقط پیام هشدار برمی‌گردد.
        /// </summary>
        private async Task<string?> TryAttachTicketInfoAsync(
            AtmScanDayDto report, Atm atm, AtmDailyAnalysis analysis, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(report.Tk))
                return null;

            var lookup = await _ticketProvider.GetByTicketNumberAsync(report.Tk, ct);

            if (!lookup.Success || lookup.Data is null)
                return lookup.Message;

            var t = lookup.Data;

            if (!SerialsMatch(t.SerialNo, report.S))
            {
                _logger.LogWarning(
                    "عدم تطابق سریال. تیکت={TicketNo} سریال‌ERDB='{ErdbSerial}' سریال‌دستگاه='{DeviceSerial}'",
                    t.RequestNo, t.SerialNo, report.S);

                return $"سریال تیکت ({t.SerialNo}) با سریال دستگاه ({report.S}) مطابقت ندارد.";
            }

            atm.UpdateLocationInfo(
                t.MInvCode, t.DeviceName, t.StateCode, t.StateName,
                t.CityName, t.SupervisionStateName, t.CustomerName,
                t.BranchCode, t.BranchName);

            var existingTicket = await _context.AtmTicketInfos
                .FirstOrDefaultAsync(x => x.DailyAnalysisId == analysis.Id, ct);

            if (existingTicket is null)
            {
                _context.AtmTicketInfos.Add(AtmTicketInfo.Create(
                    analysis.Id, t.RequestNo, t.CallDate, t.ReferDate,
                    t.ReferEndTime, t.AssignType, t.TechName));
            }
            else
            {
                existingTicket.Refresh(t.CallDate, t.ReferDate,
                    t.ReferEndTime, t.AssignType, t.TechName);
            }

            return null;
        }

        private static bool SerialsMatch(string? erdbSerial, string? deviceSerial)
            => Canonical(erdbSerial) is { Length: > 0 } a
            && Canonical(deviceSerial) is { Length: > 0 } b
            && a == b;

        /// <summary>حذف فاصله، خط تیره و یکسان‌سازی حروف بزرگ برای مقایسه سریال.</summary>
        private static string Canonical(string? s)
            => string.IsNullOrWhiteSpace(s)
                ? string.Empty
                : new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }
}