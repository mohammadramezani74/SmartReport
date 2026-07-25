using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Atm.Submit;
using SmartReportLog.Persistance;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartReportLog.Services.atm.Command.SaveData
{
    public sealed class AtmIngestionService : IAtmIngestionService
    {
        private readonly SmartLogContext _context;

        public AtmIngestionService(SmartLogContext context)
        {
            _context = context;
        }

        public async Task<IngestResult> IngestAsync(AtmScanDayDto report, CancellationToken ct)
        {
            if (report == null || string.IsNullOrWhiteSpace(report.S))
                return new IngestResult(false, "داده نامعتبر است.");

            if (!DateOnly.TryParse(report.Dt.Split(',').First(), out var start) ||
                !DateOnly.TryParse(report.Dt.Split(',').Last(), out var end))
                return new IngestResult(false, "فرمت تاریخ نامعتبر است.");

            try
            {
                var jsonDocument = await _context.jsonDocuments
                    .Where(x => x.StartDate == start && x.EndDate == end && x.SerialNumber.Trim().ToLower() == report.S.Trim().ToLower())
                    .FirstOrDefaultAsync();
                var content = JsonSerializer.Serialize(report);
                if (jsonDocument == null)
                {
                    var json = JsonDocuments.Create(start, end, content, report.S);
                    _context.jsonDocuments.Add(json);
                }
                else
                {
                    jsonDocument.Edit(content);
                }

                var atm = await _context.Atms
                    .Include(a => a.DailyAnalyses)
                        .ThenInclude(x => x.Cassettes)
                    .Include(a => a.DailyAnalyses)
                        .ThenInclude(x => x.HardwareErrors)
                    .Include(a => a.DailyAnalyses)
                        .ThenInclude(x => x.TodayErrors)
                    .FirstOrDefaultAsync(a => a.SerialNumber == report.S, ct);

                if (atm is null)
                {
                    atm = Atm.Create(report.S);
                    await _context.Atms.AddAsync(atm, ct);
                }

                atm.UpdateHardwareInfo(report.Cm, report.Os);
                int? cpuTemp = report.Ct >= 0 ? report.Ct : (int?)null;

                // پارس کردن خطاهای روزانه ورودی (Dt تکی هر آیتم)
                var parsedTodayErrors = new List<(string Device, string ErrorCode, int Count, DateOnly Date)>();
                foreach (var e in report.TodayEr)
                {
                    if (DateOnly.TryParse(e.Dt, out var errDate))
                        parsedTodayErrors.Add((e.D, e.E, e.C, errDate));
                }

                var Report = await _context.DailyAnalyses.FirstOrDefaultAsync(d => d.Date == start && d.EndDate == end && d.AtmId == atm.Id);
                if (Report != null)
                {
                    Report.Date = start;
                    Report.EndDate = end;
                    Report.TotalCards = report.Ca;
                    Report.TotalTransactions = report.Tr;
                    Report.ReceiptCount = report.Re;
                    Report.AuiSeconds = report.Au;
                    Report.DailyDispenseTotal = report.Di;
                    Report.DailyRejectTotal = report.Rj;
                    Report.ModifiedDate = DateTime.Now;
                    Report.CpuUsagePercent = report.Cu;
                    Report.RamTotalGb = report.Rt;
                    Report.RamUsedGb = report.Ru;
                    Report.CpuTemperatureC = (cpuTemp.HasValue && cpuTemp.Value >= 0) ? cpuTemp : null;
                    Report.DiskTotalGb = report.Dst;
                    Report.DiskUsedGb = report.Du;
                    Report.GayaVersion = report.Ver;

                    _context.RemoveRange(Report.Cassettes);
                    _context.RemoveRange(Report.HardwareErrors);
                    foreach (var item in report.Cs)
                    {
                        var newcasset = AtmCassetteDaily.Create(Report.Id, item.I, item.Dn, item.Pk, item.Dp, item.Rj, item.Lk);
                        _context.DailyCassettes.Add(newcasset);
                    }
                    foreach (var e in report.Er)
                    {
                        var newError = AtmHardwareErrorDaily.Create(Report.Id, e.D, e.E, e.C);
                        _context.DailyHardwareErrors.Add(newError);
                    }

                    // فقط موارد جدید (بر اساس Device+ErrorCode+Date) رو اضافه کن، تکراری‌ها رد بشن
                    var existingKeys = Report.TodayErrors
                        .Select(t => (t.Device, t.ErrorCode, t.Date))
                        .ToHashSet();

                    foreach (var t in parsedTodayErrors)
                    {
                        var key = (t.Device, t.ErrorCode, t.Date);
                        if (existingKeys.Contains(key))
                            continue;

                        var newTodayError = AtmTodayError.Create(Report.Id, t.Device, t.ErrorCode, t.Count, t.Date);
                        _context.AtmTodayErrors.Add(newTodayError);
                        existingKeys.Add(key);
                    }
                }
                else
                {
                    var detail = atm.UpsertDailyAnalysis(
                        start, end, report.Ca, report.Tr, report.Re, report.Au, report.Di, report.Rj,
                        report.Cs.Select(c => (c.I, c.Dn, c.Pk, c.Dp, c.Rj, c.Lk)),
                        report.Er.Select(e => (e.D, e.E, e.C)),
                        parsedTodayErrors,
                        report.Cu, report.Rt, report.Ru, cpuTemp, report.Dst, report.Du, report.Ver, report.Tk, report.Pc);
                    _context.DailyAnalyses.Add(detail);
                }

                await _context.SaveChangesAsync(ct);
                return new IngestResult(true, $"گزارش {start} تا {end} برای دستگاه {report.S} ثبت شد.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
