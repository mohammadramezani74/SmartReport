using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Model.Sana;
using SmartReportLog.Persistance;
using SmartReportLog.Services.Ticket;

namespace SmartReportLog.Services.Sana
{
    public sealed class SanaArchiveService : ISanaArchiveService
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private readonly SmartLogContext _context;
        private readonly SanaStorageOptions _options;
        private readonly ILogger<SanaArchiveService> _logger;
        private readonly ITicketInfoProvider _ticketProvider;

        public SanaArchiveService(
            SmartLogContext context,
            Microsoft.Extensions.Options.IOptions<SanaStorageOptions> options,
            ILogger<SanaArchiveService> logger,
            ITicketInfoProvider ticketProvider)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
            _ticketProvider = ticketProvider;
        }

        // ================================================================
        // 1) دریافت و ذخیره فایل زیپ
        // ================================================================
        public async Task<TotalUploadResponse> IngestAsync(
            Stream zipStream, string originalFileName, CancellationToken ct)
        {
            Directory.CreateDirectory(_options.RootPath);

            // فایل ابتدا کامل روی دیسک موقت نوشته می‌شود تا بتوان چند بار خواند
            var tempPath = Path.Combine(Path.GetTempPath(), $"sana_{Guid.NewGuid():N}.zip");

            try
            {
                long size;
                string hash;

                await using (var temp = File.Create(tempPath))
                {
                    await zipStream.CopyToAsync(temp, ct);
                    size = temp.Length;
                }

                if (size == 0)
                    return new TotalUploadResponse(false, "فایل ارسالی خالی است.");

                if (size > _options.MaxFileSizeBytes)
                    return new TotalUploadResponse(false,
                        $"حجم فایل بیش از حد مجاز است ({_options.MaxFileSizeBytes / 1024 / 1024} مگابایت).");

                await using (var forHash = File.OpenRead(tempPath))
                    hash = Convert.ToHexString(await SHA256.HashDataAsync(forHash, ct));

                ParsedArchive parsed;

                try
                {
                    using var zip = ZipFile.OpenRead(tempPath);
                    parsed = Parse(zip, originalFileName);
                }
                catch (InvalidDataException)
                {
                    return new TotalUploadResponse(false, "فایل ارسالی یک آرشیو زیپ معتبر نیست.");
                }
                if (parsed.Error is not null)
                    return new TotalUploadResponse(false, parsed.Error);
                var lookup = await _ticketProvider.GetByTicketNumberAsync(parsed.TicketNumber, ct);

                if (!lookup.Success || lookup.Data is null)
                    return new TotalUploadResponse(false,
                        lookup.Message ?? $"تیکت {parsed.TicketNumber} در سامانه یافت نشد.");

                var serial = lookup.Data.SerialNo?.Trim();

                if (string.IsNullOrWhiteSpace(serial))
                    return new TotalUploadResponse(false,
                        $"برای تیکت {parsed.TicketNumber} سریال معتبری در سامانه ثبت نشده است.");

                if (!string.IsNullOrWhiteSpace(parsed.SerialNumber)
                    && !SerialsMatch(serial, parsed.SerialNumber))
                {
                    // فایل رد نمی‌شود، اما اختلاف برای پیگیری ثبت می‌شود
                    _logger.LogWarning(
                        "سریال فایل با سریال تیکت متفاوت است. تیکت={Ticket} ERDB='{Erdb}' فایل='{File}'",
                        parsed.TicketNumber, serial, parsed.SerialNumber);
                }

                // ---------- دستگاه ----------
                var atm = await _context.Atms
                    .FirstOrDefaultAsync(a => a.SerialNumber == serial, ct);

                if (atm is null)
                {
                    atm = Atm.Create(serial);
                    await _context.Atms.AddAsync(atm, ct);
                    await _context.SaveChangesAsync(ct);
                }


                    // ---------- جایگزینی گزارش قبلی با همان تیکت و بازه ----------
                    var existing = await _context.AtmTotalReports
                    .FirstOrDefaultAsync(r => r.TicketNumber == parsed.TicketNumber
                                           && r.FirstLogDate == parsed.FirstLogDate
                                           && r.LastLogDate == parsed.LastLogDate, ct);

                bool replaced = existing is not null;

                if (existing is not null)
                {
                    DeleteStoredFile(existing.StoredFileName);
                    _context.AtmTotalReports.Remove(existing);
                    await _context.SaveChangesAsync(ct);
                }

                // ---------- ذخیره فایل نهایی ----------
                var storedName = BuildStoredName(parsed.TicketNumber!, serial, hash);
                var finalPath = Path.Combine(_options.RootPath, storedName);

                File.Move(tempPath, finalPath, overwrite: true);

                // ---------- ثبت در دیتابیس ----------
                var report = AtmTotalReport.Create(
                    atm.Id, serial, parsed.TicketNumber!,
                    parsed.FirstLogDate, parsed.LastLogDate, parsed.ExportDate,
                    parsed.Total!.TotalCards, parsed.Total.TotalTransactions,
                    parsed.Total.TotalReceipts, parsed.Total.TotalReject,
                    parsed.Total.TotalDispense,
                    parsed.Total.Cassettes.Sum(c => c.TotalDispense),
                    parsed.BankName,
                    storedName, originalFileName, size, hash, parsed.DailyFileCount);

                foreach (var c in parsed.Total.Cassettes)
                {
                    report.AddCassette(AtmTotalCassette.Create(
                        report.Id, c.Id, c.Denomination, c.InitialCount,
                        c.TotalPickup, c.TotalDispense, c.TotalReject, c.LastKnownCount));
                }

                foreach (var e in parsed.Total.HardwareErrors)
                {
                    var error = AtmTotalError.Create(
                        report.Id,
                        string.IsNullOrWhiteSpace(e.Device) ? "UNKNOWN" : e.Device.Trim(),
                        string.IsNullOrWhiteSpace(e.ErrorCode) ? "UNKNOWN" : e.ErrorCode.Trim(),
                        e.Description?.Trim(),
                        e.Count);

                    // آرایه Dates تکراری است — به (تاریخ، تعداد) تجمیع می‌شود
                    foreach (var group in e.Dates
                        .Select(ParseDate)
                        .Where(d => d.HasValue)
                        .GroupBy(d => d!.Value))
                    {
                        error.AddDate(group.Key, group.Count());
                    }

                    report.AddError(error);
                }
                report.Document = AtmTotalDocument.Create(
    report.Id,
    parsed.RawTotalJson!,
    parsed.RawInfoJson,
    parsed.RawConfigJson,
    parsed.RawDenominationJson);
                _context.AtmTotalReports.Add(report);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "گزارش توتال ثبت شد. تیکت={Ticket} سریال={Serial} بازه={From}..{To}",
                    report.TicketNumber, report.SerialNumber, report.FirstLogDate, report.LastLogDate);

                return new TotalUploadResponse(
                    true,
                    replaced ? "گزارش قبلی جایگزین شد." : "گزارش با موفقیت ثبت شد.",
                    report.Id, report.TicketNumber, report.SerialNumber,
                    report.FirstLogDate.ToString("yyyy-MM-dd"),
                    report.LastLogDate.ToString("yyyy-MM-dd"),
                    report.DailyFileCount, replaced);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش فایل توتال {FileName}", originalFileName);
                return new TotalUploadResponse(false, "پردازش فایل با خطا مواجه شد.");
            }
            finally
            {
                if (File.Exists(tempPath))
                    try { File.Delete(tempPath); } catch { /* پاکسازی بهترین‌تلاش */ }
            }
        }
        private static bool SerialsMatch(string? a, string? b)
    => Canonical(a) is { Length: > 0 } x
    && Canonical(b) is { Length: > 0 } y
    && x == y;

        private static string Canonical(string? s)
            => string.IsNullOrWhiteSpace(s)
                ? string.Empty
                : new string(s.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        // ================================================================
        // 2) لیست آرشیو با فیلتر
        // ================================================================
        public async Task<TotalArchiveListResponse> GetArchivesAsync(
            string? ticketNumber, string? serialNumber,
            DateOnly? from, DateOnly? to,
            int page, int pageSize, CancellationToken ct)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

            var query = _context.AtmTotalReports.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketNumber))
            {
                var t = ticketNumber.Trim();
                query = query.Where(r => r.TicketNumber == t);
            }

            if (!string.IsNullOrWhiteSpace(serialNumber))
            {
                var s = serialNumber.Trim();
                query = query.Where(r => r.SerialNumber == s);
            }

            // هر گزارشی که بازه‌اش با بازه درخواستی همپوشانی دارد
            if (from.HasValue)
                query = query.Where(r => r.LastLogDate >= from.Value);

            if (to.HasValue)
                query = query.Where(r => r.FirstLogDate <= to.Value);

            var total = await query.CountAsync(ct);

            var rows = await query
                .OrderByDescending(r => r.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.TicketNumber,
                    r.SerialNumber,
                    r.FirstLogDate,
                    r.LastLogDate,
                    r.ExportDate,
                    r.UploadedAt,
                    r.FileSizeBytes,
                    r.DailyFileCount
                })
                .ToListAsync(ct);

            var items = rows.Select(r => new TotalArchiveItem(
                r.Id, r.TicketNumber, r.SerialNumber,
                r.FirstLogDate.ToString("yyyy-MM-dd"),
                r.LastLogDate.ToString("yyyy-MM-dd"),
                r.ExportDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                r.UploadedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                r.FileSizeBytes, r.DailyFileCount,
                $"/api/sana/total/{r.Id}/download")).ToList();

            return new TotalArchiveListResponse(total, page, pageSize, items);
        }

        public async Task<(Stream? Stream, string? FileName)> OpenArchiveAsync(
            Guid reportId, CancellationToken ct)
        {
            var report = await _context.AtmTotalReports.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reportId, ct);

            if (report is null) return (null, null);

            var path = Path.Combine(_options.RootPath, report.StoredFileName);

            if (!File.Exists(path))
            {
                _logger.LogWarning("فایل آرشیو روی دیسک یافت نشد: {Path}", path);
                return (null, null);
            }

            return (File.OpenRead(path), report.OriginalFileName);
        }

        // ================================================================
        // 3) وضعیت تیکت
        // ================================================================
        public async Task<TicketStatusResponse> GetTicketStatusAsync(
            string ticketNumber, CancellationToken ct)
        {
            var ticket = ticketNumber?.Trim() ?? string.Empty;

            var dailyRows = await _context.DailyAnalyses.AsNoTracking()
                .Where(d => d.TicketNumber == ticket)
                .Select(d => new
                {
                    d.Date,
                    d.EndDate,
                    d.TotalTransactions,
                    ErrorCount = d.HardwareErrors.Sum(e => (int?)e.Count) ?? 0,
                    Serial = _context.Atms
                        .Where(a => a.Id == d.AtmId)
                        .Select(a => a.SerialNumber)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var totalRow = await _context.AtmTotalReports.AsNoTracking()
                .Where(r => r.TicketNumber == ticket)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => new
                {
                    r.Id,
                    r.SerialNumber,
                    r.FirstLogDate,
                    r.LastLogDate,
                    r.UploadedAt,
                    r.TotalTransactions,
                    r.TotalCards,
                    r.DailyFileCount
                })
                .FirstOrDefaultAsync(ct);

            bool hasDaily = dailyRows.Count > 0;
            bool hasTotal = totalRow is not null;

            var status = (hasDaily, hasTotal) switch
            {
                (true, true) => TicketDataStatus.Both,
                (true, false) => TicketDataStatus.DailyOnly,
                (false, true) => TicketDataStatus.TotalOnly,
                _ => TicketDataStatus.None
            };

            var statusText = status switch
            {
                TicketDataStatus.Both => "گزارش هفتگی و توتال هر دو ثبت شده است.",
                TicketDataStatus.DailyOnly => "فقط گزارش هفتگی (اسکن) ثبت شده است؛ گزارش توتال ارسال نشده.",
                TicketDataStatus.TotalOnly => "فقط گزارش توتال ثبت شده است؛ اسکن هفتگی انجام نشده.",
                _ => "برای این شماره تیکت هیچ گزارشی ثبت نشده است."
            };

            TicketDailySummary? daily = null;

            if (hasDaily)
            {
                daily = new TicketDailySummary(
                    dailyRows.Count,
                    dailyRows.Min(d => d.Date).ToString("yyyy-MM-dd"),
                    dailyRows.Max(d => d.EndDate).ToString("yyyy-MM-dd"),
                    dailyRows.Select(d => d.Serial).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)),
                    dailyRows.Sum(d => d.TotalTransactions),
                    dailyRows.Sum(d => d.ErrorCount));
            }

            TicketTotalSummary? total = null;

            if (totalRow is not null)
            {
                total = new TicketTotalSummary(
                    totalRow.Id, totalRow.SerialNumber,
                    totalRow.FirstLogDate.ToString("yyyy-MM-dd"),
                    totalRow.LastLogDate.ToString("yyyy-MM-dd"),
                    totalRow.UploadedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalRow.TotalTransactions, totalRow.TotalCards,
                    totalRow.DailyFileCount,
                    $"/api/sana/total/{totalRow.Id}/download");
            }

            // تیکت تنها زمانی قابل بستن است که گزارش توتال دریافت شده باشد
            return new TicketStatusResponse(
                ticket, status, statusText, hasDaily, hasTotal,
                CanClose: hasTotal, daily, total);
        }
        private static string? ReadRaw(ZipArchive zip, string fileName)
        {
            var entry = zip.Entries.FirstOrDefault(e =>
                string.Equals(Path.GetFileName(e.FullName), fileName, StringComparison.OrdinalIgnoreCase));

            if (entry is null) return null;

            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        // ================================================================
        // استخراج محتوای زیپ
        // ================================================================
        private sealed record ParsedArchive
        {
            public string? Error { get; init; }
            public string? TicketNumber { get; init; }
            public string? SerialNumber { get; init; }
            public string? BankName { get; init; }
            public DateOnly FirstLogDate { get; init; }
            public DateOnly LastLogDate { get; init; }
            public DateTime? ExportDate { get; init; }
            public SanaTotalFile? Total { get; init; }
            public int DailyFileCount { get; init; }
            public string? RawTotalJson { get; init; }
            public string? RawInfoJson { get; init; }
            public string? RawConfigJson { get; init; }
            public string? RawDenominationJson { get; init; }
        }

        private static ParsedArchive Parse(ZipArchive zip, string originalFileName)
        {
            // ساختار دو حالت دارد: فایل‌ها در ریشه، یا زیر <name>/Output/
            var info = ReadJson<SanaInfoFile>(zip, "info.json");
            var config = ReadJson<SanaConfigFile>(zip, "config.json");
            var total = ReadJson<SanaTotalFile>(zip, "total.json");

            if (total is null)
                return new ParsedArchive { Error = "فایل total.json در آرشیو یافت نشد." };

            var dailyDates = zip.Entries
                .Select(e => Path.GetFileName(e.FullName))
                .Where(n => n.StartsWith("daily_", StringComparison.OrdinalIgnoreCase)
                         && n.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                .Select(n => ParseDate(n[6..^5]))
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();

            // شماره تیکت: info.json، سپس config.json، سپس نام فایل
            var ticket = FirstNonEmpty(
                info?.TicketNumber,
                config?.TicketNumber,
                Path.GetFileNameWithoutExtension(originalFileName));

            if (string.IsNullOrWhiteSpace(ticket) || !long.TryParse(ticket.Trim(), out _))
                return new ParsedArchive { Error = "شماره تیکت در آرشیو یافت نشد یا معتبر نیست." };

            // سریال: total.json، سپس config.json، سپس نام پوشه ریشه
            var serial = FirstNonEmpty(
                total.AtmSerial,
                config?.AtmSerial,
                RootFolderName(zip));


            // بازه: info.json، در غیر این صورت از نام فایل‌های daily
            var first = ParseDate(info?.FirstLogDate) ?? (dailyDates.Count > 0 ? dailyDates.Min() : null);
            var last = ParseDate(info?.LastLogDate) ?? (dailyDates.Count > 0 ? dailyDates.Max() : null);

            if (first is null || last is null)
                return new ParsedArchive { Error = "بازه تاریخ گزارش قابل تشخیص نیست." };

            if (last < first)
                return new ParsedArchive { Error = "تاریخ پایان کوچک‌تر از تاریخ شروع است." };

            return new ParsedArchive
            {
                TicketNumber = ticket.Trim(),
                SerialNumber = serial.Trim(),
                BankName = config?.SelectedBank?.Trim(),
                FirstLogDate = first.Value,
                LastLogDate = last.Value,
                ExportDate = ParseDateTime(info?.ExportDate),
                Total = total,
                DailyFileCount = dailyDates.Count,
                RawTotalJson = ReadRaw(zip, "total.json"),
                RawInfoJson = ReadRaw(zip, "info.json"),
                RawConfigJson = ReadRaw(zip, "config.json"),
                RawDenominationJson = ReadRaw(zip, "denomination.json"),
            };
        }

        /// <summary>فایل را در هر عمقی از آرشیو پیدا و دیسریالایز می‌کند.</summary>
        private static T? ReadJson<T>(ZipArchive zip, string fileName) where T : class
        {
            var entry = zip.Entries.FirstOrDefault(e =>
                string.Equals(Path.GetFileName(e.FullName), fileName, StringComparison.OrdinalIgnoreCase));

            if (entry is null) return null;

            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), JsonOpts);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? RootFolderName(ZipArchive zip)
        {
            var withFolder = zip.Entries.FirstOrDefault(e => e.FullName.Contains('/'));
            return withFolder?.FullName.Split('/')[0];
        }

        private static string? FirstNonEmpty(params string?[] values)
            => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        private static DateOnly? ParseDate(string? raw)
            => DateOnly.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                   System.Globalization.DateTimeStyles.None, out var d) ? d : null;

        private static DateTime? ParseDateTime(string? raw)
            => DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                   System.Globalization.DateTimeStyles.None, out var d) ? d : null;

        private static string BuildStoredName(string ticket, string serial, string hash)
            => $"{Sanitize(ticket)}_{Sanitize(serial)}_{hash[..12]}.zip";

        private static string Sanitize(string value)
            => new(value.Where(char.IsLetterOrDigit).ToArray());

        private void DeleteStoredFile(string storedName)
        {
            var path = Path.Combine(_options.RootPath, storedName);
            if (!File.Exists(path)) return;

            try { File.Delete(path); }
            catch (Exception ex) { _logger.LogWarning(ex, "حذف فایل قدیمی ناموفق بود: {Path}", path); }
        }
    }
}