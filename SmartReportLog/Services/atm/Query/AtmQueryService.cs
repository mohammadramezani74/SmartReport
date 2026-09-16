using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Atm.Queries;
using SmartReportLog.Model.Ticket;
using SmartReportLog.Persistance;
using SmartReportLog.Services.Auth;
using SmartReportLog.Utilities.DateTimeHalper;

namespace SmartReportLog.Services.atm.Query
{
    public sealed class AtmQueryService : IAtmQueryService
    {
        private readonly SmartLogContext _context;
        private readonly IUserScopeService _scopeService;

        public AtmQueryService(SmartLogContext context, IUserScopeService scopeService)
        {
            _context = context;
            _scopeService = scopeService;
        }

        public async Task<(List<AtmListItemDto> Items, int TotalCount)> GetAtmListAsync(
      string? search, int? stateCode, bool onlyMissingLocation,
      int page, int pageSize, CancellationToken ct)
        {
            var query = ScopedAtms().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(a =>
                    a.SerialNumber!.Contains(s) ||
                    a.BranchName!.Contains(s) ||
                    a.BranchCode!.Contains(s) ||
                    a.CityName!.Contains(s));
            }

            if (onlyMissingLocation)
                query = query.Where(a => a.StateCode == null);
            else if (stateCode.HasValue)
                query = query.Where(a => a.StateCode == stateCode.Value);

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .Select(a => new
                {
                    a.Id,
                    a.SerialNumber,
                    a.StateCode,
                    a.StateName,
                    a.CityName,
                    a.BranchName,
                    a.DeviceName,
                    LastReportEnd = a.DailyAnalyses
                        .OrderByDescending(p => p.EndDate)
                        .Select(p => (DateOnly?)p.EndDate)
                        .FirstOrDefault(),
                    TotalReportsCount = a.DailyAnalyses.Count()
                })
                .OrderByDescending(x => x.LastReportEnd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AtmListItemDto(
                    x.Id, x.SerialNumber!, x.LastReportEnd, x.TotalReportsCount,
                    x.StateCode, x.StateName, x.CityName, x.BranchName, x.DeviceName))
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<List<AtmStateOptionDto>> GetStateOptionsAsync(CancellationToken ct)
        {
            var rows = await ScopedAtms()
                .Where(a => a.StateCode != null && a.StateName != null)
                .GroupBy(a => new { Code = a.StateCode!.Value, Name = a.StateName! })
                .Select(g => new { g.Key.Code, g.Key.Name, Count = g.Count() })
                .OrderBy(x => x.Name)
                .ToListAsync(ct);

            return rows
                .Select(x => new AtmStateOptionDto(x.Code, x.Name, x.Count))
                .ToList();
        }
        public async Task<AtmDetailDto?> GetAtmDetailAsync(Guid atmId, DateOnly? from, DateOnly? to, CancellationToken ct)
        {
            if (!await IsAtmInScopeAsync(atmId, ct)) return null;
            var atm = await _context.Atms
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.DailyAnalyses).ThenInclude(p => p.TicketInfo)
            .Include(a => a.DailyAnalyses).ThenInclude(p => p.Cassettes)
            .Include(a => a.DailyAnalyses).ThenInclude(p => p.HardwareErrors)
            .Include(a => a.DailyAnalyses).ThenInclude(p => p.TodayErrors)
            .FirstOrDefaultAsync(a => a.Id == atmId, ct);

            if (atm is null) return null;

            var periods = atm.DailyAnalyses.AsEnumerable();
            if (from.HasValue) periods = periods.Where(p => p.EndDate >= from.Value);
            if (to.HasValue) periods = periods.Where(p => p.Date <= to.Value);

            var orderedPeriods = periods.OrderBy(p => p.EndDate).ToList();
            var latest = orderedPeriods.LastOrDefault();

            var points = orderedPeriods.Select(p => new AtmPeriodPointDto(
                p.Date, p.EndDate, p.TotalCards, p.TotalTransactions, p.ReceiptCount,
                (int)p.DailyDispenseTotal, (int)p.DailyRejectTotal, p.CpuTemperatureC)).ToList();

            var errors = latest?.HardwareErrors
          .GroupBy(x => new
          {
              x.Device,
              x.ErrorCode
          })
          .Select(g => new AtmErrorSummaryDto(
              g.Key.Device,
              g.Key.ErrorCode,
              g.Sum(x => x.Count)))
          .OrderByDescending(x => x.Count)
          .ToList() ?? new();

            var cassettes = latest?.Cassettes
                .GroupBy(x => new
                {
                    x.CassetteNumber,
                    x.Denomination
                })
                .Select(g =>
                {
                    var c = g.First();

                    return new AtmCassetteSummaryDto(
                        c.CassetteNumber,
                        c.Denomination,
                        g.Sum(x => x.TotalPickup),
                        g.Sum(x => x.TotalDispense),
                        g.Sum(x => x.TotalReject));
                })
                .OrderBy(x => x.CassetteIndex)
                .ToList() ?? new();

            // خطاهای روزانه در کل بازه‌ی فیلترشده (نه فقط آخرین گزارش)
            var todayErrors = orderedPeriods
                .SelectMany(p => p.TodayErrors)
                .Select(e => new AtmTodayErrorDto(e.Device, e.ErrorCode, e.Count, e.Date))
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Count)
                .ToList();
            return new AtmDetailDto(
                atm.Id, atm.SerialNumber!, atm.CpuModel, atm.OsVersion,
                points, errors, cassettes, todayErrors,
                latest?.CpuUsagePercent ?? 0, latest?.RamTotalGb ?? 0, latest?.RamUsedGb ?? 0, latest?.CpuTemperatureC,
                latest?.DiskTotalGb ?? 0, latest?.DiskUsedGb ?? 0, latest?.GayaVersion ?? "-", latest?.ImageVersion ?? "-",
                BuildLocation(atm.MInvCode, atm.DeviceName, atm.StateCode, atm.StateName,
                    atm.CityName, atm.SupervisionStateName, atm.CustomerName, atm.BranchCode, atm.BranchName),
                BuildTicket(latest));
        }

        public async Task<(List<AtmPeriodListItemDto> Items, int TotalCount)> GetAtmPeriodsAsync(
         Guid atmId, int page, int pageSize, CancellationToken ct)
        {
            if (!await IsAtmInScopeAsync(atmId, ct))
                return (new List<AtmPeriodListItemDto>(), 0);
            var query = _context.Set<AtmDailyAnalysis>()
                .AsNoTracking()
                .Where(p => p.AtmId == atmId);

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(p => p.EndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new AtmPeriodListItemDto(
                    p.Id,DateTime.Parse( p.Date.ToString()).ToFarsi(), DateTime.Parse(p.EndDate.ToString()).ToFarsi(),
                    p.TotalTransactions, p.TotalCards, p.ReceiptCount,
                    p.HardwareErrors.Sum(e => (int?)e.Count) ?? 0,p.TicketNumber))
                .ToListAsync(ct);

            return (items, totalCount);
        }
        public async Task<AtmDetailDto?> GetAtmPeriodDetailAsync(Guid atmId, Guid periodId, CancellationToken ct)
        {
            var atm = await _context.Atms
                .AsNoTracking()
                .Where(a => a.Id == atmId)
                .Select(a => new
                {
                    a.Id,
                    a.SerialNumber,
                    a.CpuModel,
                    a.OsVersion,
                    a.MInvCode,
                    a.DeviceName,
                    a.StateCode,
                    a.StateName,
                    a.CityName,
                    a.SupervisionStateName,
                    a.CustomerName,
                    a.BranchCode,
                    a.BranchName
                })
                .FirstOrDefaultAsync(ct);

            var latest = await _context.Set<AtmDailyAnalysis>()
                .AsNoTracking()
                .Include(p => p.Cassettes)
                .Include(p => p.HardwareErrors)
                .Include(p => p.TodayErrors)
                .Include(p => p.TicketInfo)
                .FirstOrDefaultAsync(p => p.Id == periodId && p.AtmId == atmId, ct);
            if (latest is null) return null;

            var points = new AtmPeriodPointDto(
             latest.Date, latest.EndDate, latest.TotalCards, latest.TotalTransactions, latest.ReceiptCount,
             (int)latest.DailyDispenseTotal, (int)latest.DailyRejectTotal,latest.CpuTemperatureC);

            var errors = latest?.HardwareErrors
          .GroupBy(x => new
          {
              x.Device,
              x.ErrorCode
          })
          .Select(g => new AtmErrorSummaryDto(
              g.Key.Device,
              g.Key.ErrorCode,
              g.Sum(x => x.Count)))
          .OrderByDescending(x => x.Count)
          .ToList() ?? new();

            var cassettes = latest?.Cassettes
                .GroupBy(x => new
                {
                    x.CassetteNumber,
                    x.Denomination
                })
                .Select(g =>
                {
                    var c = g.First();

                    return new AtmCassetteSummaryDto(
                        c.CassetteNumber,
                        c.Denomination,
                        g.Sum(x => x.TotalPickup),
                        g.Sum(x => x.TotalDispense),
                        g.Sum(x => x.TotalReject));
                })
                .OrderBy(x => x.CassetteIndex)
                .ToList() ?? new();


            var todayErrors = latest.TodayErrors
                .Select(e => new AtmTodayErrorDto(e.Device, e.ErrorCode, e.Count, e.Date))
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.Count)
                .ToList();

            return new AtmDetailDto(
     atm.Id, atm.SerialNumber!, atm.CpuModel, atm.OsVersion,
     [points], errors, cassettes, todayErrors,
     latest.CpuUsagePercent, latest.RamTotalGb, latest.RamUsedGb, latest.CpuTemperatureC,
     latest.DiskTotalGb, latest.DiskUsedGb, latest.GayaVersion ?? "-", latest?.ImageVersion ?? "-",
     BuildLocation(atm.MInvCode, atm.DeviceName, atm.StateCode, atm.StateName,
         atm.CityName, atm.SupervisionStateName, atm.CustomerName, atm.BranchCode, atm.BranchName),
     BuildTicket(latest));
        }

        public async Task<List<AtmErrorCountDto>> TopTenAtmWithMostErrors(
        int days, CancellationToken ct)
        {

            var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));

            return await ScopedAtms()
               
                .Select(a => new AtmErrorCountDto
                {
                    AtmId = a.Id,
                    SerialNumber = a.SerialNumber!,
                    ErrorCount = a.DailyAnalyses
                        .Where(p => p.EndDate >= cutoff)
                        .SelectMany(p => p.HardwareErrors)
                        .Sum(e => (int?)e.Count) ?? 0,
                    ReportCount = a.DailyAnalyses.Count(p => p.EndDate >= cutoff),
                    FirstDate = a.DailyAnalyses
                        .Where(p => p.EndDate >= cutoff)
                        .Min(p => (DateOnly?)p.Date),
                    LastDate = a.DailyAnalyses
                        .Where(p => p.EndDate >= cutoff)
                        .Max(p => (DateOnly?)p.EndDate)
                })
                .Where(x => x.ErrorCount > 0)
                .OrderByDescending(x => x.ErrorCount)
                .Take(10)
                .ToListAsync(ct);
        }

        public async Task<AtmDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
        {
            // تبدیل تاریخ امروز سیستم به DateOnly
            var today =DateTime.Today.AddDays(-7);
            var scopedAtmIds = ScopedAtms().Select(a => a.Id);

            var totalAtms = await ScopedAtms().CountAsync(cancellationToken);

       
            // ۲. تعداد دستگاه‌های فعال امروز (دستگاه‌هایی که برای امروز تحلیل روزانه دارند)
            var activeAtms = await _context.DailyAnalyses
                .Where(x => scopedAtmIds.Contains(x.AtmId) && x.CreateDate >= today)
                .Select(x => x.AtmId)
                .Distinct()
                .CountAsync(cancellationToken);

            // ۳. مجموع تراکنش‌ها و خطاهای سخت‌افزاری امروز شبکه
            var todayStats = await _context.DailyAnalyses
                .Where(x => scopedAtmIds.Contains(x.AtmId) && x.CreateDate >= today)
                .Select(x => new
                {
                    Transactions = x.TotalTransactions,
                    // جمع زدن کل خطاهای سخت‌افزاری امروز این دستگاه
                    Errors = x.HardwareErrors.Sum(e => e.Count)
                })
                .ToListAsync(cancellationToken);

            var todayTotalTransactions = todayStats.Sum(x => x.Transactions);
            var todayTotalErrors = todayStats.Sum(x => x.Errors);

            return new AtmDashboardSummaryDto(
                TotalAtmsCount: totalAtms,
                ActiveAtmsCount: activeAtms,
                TodayTotalReportsCount: todayTotalTransactions, // استفاده از مجموع تراکنش‌ها به عنوان ترافیک زنده
                TodayTotalErrorsCount: todayTotalErrors
            );
        }

        public async Task<List<AtmErrorTypeDistributionDto>> GetErrorTypeDistributionAsync(CancellationToken cancellationToken)
        {
            var sevenDaysAgo = DateTime.Today.AddDays(-7);
            var scopedAtmIds = ScopedAtms().Select(a => a.Id);
            // گروه‌بندی خطاها بر اساس ستون Device در یک هفته اخیر
            var errorData = await _context.DailyHardwareErrors
                .Where(x => _context.DailyAnalyses.Any(da =>
               scopedAtmIds.Contains(da.AtmId) && da.CreateDate.Value.Date>= sevenDaysAgo))
                .GroupBy(x => x.Device)
                .Select(g => new
                {
                    DeviceCode = g.Key,
                    TotalCount = g.Sum(x => x.Count)
                })
                .OrderByDescending(x => x.TotalCount)
                .ToListAsync(cancellationToken);

            // نگاشت کدهای مخفف دیتابیس به عناوین فارسی و خوانا
            var result = errorData.Select(x =>
            {
                var (persianName, color) = MapDeviceToPersianAndColor(x.DeviceCode);
                return new AtmErrorTypeDistributionDto(
                    ErrorCategory: persianName,
                    Count: x.TotalCount,
                    ColorHex: color
                );
            }).ToList();

            return result;
        }

        public async Task<List<StateErrorStatsDto>> GetStateErrorStatsAsync(int days, CancellationToken ct)
        {
            var cutoff = DateOnly.FromDateTime(DateTime.Now.AddDays(-days));

            var rows = await ScopedAtms()
                .Where(a => a.StateName != null)
                .Select(a => new
                {
                    a.StateCode,
                    a.StateName,
                    Errors = a.DailyAnalyses
                        .Where(p => p.EndDate >= cutoff)
                        .SelectMany(p => p.HardwareErrors)
                        .Sum(e => (int?)e.Count) ?? 0
                })
                .ToListAsync(ct);

            return rows
                .GroupBy(x => new { x.StateCode, x.StateName })
                .Select(g => new StateErrorStatsDto(
                    g.Key.StateCode, g.Key.StateName!, g.Count(), g.Sum(x => x.Errors)))
                .OrderByDescending(x => x.ErrorsPerAtm)
                .ToList();
        }
        public async Task<List<AtmTotalDetailDto>> GetTotalReportsAsync(Guid atmId, CancellationToken ct)
        {
            if (!await IsAtmInScopeAsync(atmId, ct))
                return new List<AtmTotalDetailDto>();
            var reports = await _context.AtmTotalReports
                .AsNoTracking()
                .Where(r => r.AtmId == atmId)
                .Include(r => r.Cassettes)
                .Include(r => r.Errors).ThenInclude(e => e.Dates)
                .OrderByDescending(r => r.LastLogDate)
                .ToListAsync(ct);

            return reports.Select(r => new AtmTotalDetailDto(
                r.Id, r.TicketNumber, r.SerialNumber,
                r.FirstLogDate, r.LastLogDate, r.ExportDate, r.UploadedAt, r.BankName,
                r.TotalCards, r.TotalTransactions, r.TotalReceipts, r.TotalReject,
                r.TotalDispenseComputed, r.DailyFileCount, r.FileSizeBytes,
                r.Cassettes.OrderBy(c => c.CassetteId)
                    .Select(c => new AtmTotalCassetteDto(
                        c.CassetteId, c.Denomination, c.InitialCount,
                        c.TotalPickup, c.TotalDispense, c.TotalReject, c.LastKnownCount))
                    .ToList(),
                r.Errors.OrderByDescending(e => e.Count)
                    .Select(e => new AtmTotalErrorDto(
                        e.Device, e.ErrorCode, e.Description, e.Count,
                        e.Dates.OrderBy(d => d.Date)
                            .Select(d => new AtmTotalErrorPointDto(d.Date, d.Count))
                            .ToList()))
                    .ToList()
            )).ToList();
        }
        private (string PersianName, string ColorHex) MapDeviceToPersianAndColor(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                return ("خطای نامشخص", "#64748b");

            return deviceName.Trim().ToUpper() switch
            {
                "HOST" => ("ارتباط با مرکز (Network/Host)", "#ef4444"),       // قرمز برای قطعی مرکز و تایم‌اوت
                "RPR" => ("پرینتر رسید (Receipt Printer)", "#10b981"),      // سبز برای خطاهای رسید مشتری
                "CDM" or "DISPENSER" => ("بخش اسکناس‌شمار (Dispenser)", "#f59e0b"), // نارنجی
                "IDC" or "CARDREADER" => ("کارت‌خوان (Card Reader)", "#3b82f6"),  // آبی
                "PIN" or "EPP" => ("صفحه کلید امن (Pinpad)", "#8b5cf6"),        // بنفش
                "SIU" => ("سنسورها و ماژول باینری (SIU)", "#ec4899"),       // صورتی
                _ => ($"سخت‌افزار ({deviceName})", "#64748b")             // خاکستری برای بقیه موارد پیش‌بینی نشده
            };
        }

        private static AtmTicketDetailDto? BuildTicket(AtmDailyAnalysis? period)
        {
            if (period is null) return null;

            var t = period.TicketInfo;
            if (t is null && string.IsNullOrWhiteSpace(period.TicketNumber)) return null;

            return new AtmTicketDetailDto(
                period.TicketNumber, period.PersonnelCode,
                t?.RequestNo, t?.CallDate, t?.ReferDate, t?.ReferEndTime,
                t?.AssignType, t?.TechName, t?.FetchedAt);
        }

        private static AtmLocationDto BuildLocation(
            string? mInv, string? device, int? stateCode, string? stateName,
            string? city, string? supervision, string? customer, string? branchCode, string? branchName)
            => new(mInv, device, stateCode, stateName, city, supervision, customer, branchCode, branchName);
        /// <summary>
        /// دستگاه‌های قابل مشاهده برای کاربر جاری.
        /// همه کوئری‌ها باید از این متد شروع شوند، نه مستقیم از _context.Atms
        /// </summary>
        private IQueryable<Atm> ScopedAtms()
        {
            var scope = _scopeService.GetScope();

            var query = _context.Atms.AsNoTracking();

            return scope.IsRestricted
                ? query.Where(a => a.StateCode == scope.StateCode)
                : query;
        }

        /// <summary>آیا دستگاه موردنظر در محدوده دسترسی کاربر جاری است؟</summary>
        private async Task<bool> IsAtmInScopeAsync(Guid atmId, CancellationToken ct)
        {
            var scope = _scopeService.GetScope();

            if (!scope.IsRestricted) return true;

            return await _context.Atms
                .AsNoTracking()
                .AnyAsync(a => a.Id == atmId && a.StateCode == scope.StateCode, ct);
        }
    }
}
