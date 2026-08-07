using System.Text.Json.Serialization;

namespace SmartReportLog.Model.Sana
{
    // ---------- مدل‌های فایل‌های داخل زیپ ----------

    public sealed class SanaInfoFile
    {
        public string? TicketNumber { get; set; }
        public string? FirstLogDate { get; set; }
        public string? LastLogDate { get; set; }
        public string? ExportDate { get; set; }
    }

    public sealed class SanaConfigFile
    {
        public string? TicketNumber { get; set; }
        public string? AtmSerial { get; set; }
        public string? SelectedBank { get; set; }
    }

    public sealed class SanaTotalFile
    {
        public string? AtmSerial { get; set; }
        public int TotalCards { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalReceipts { get; set; }
        public int TotalDispense { get; set; }
        public int TotalReject { get; set; }
        public List<SanaTotalError> HardwareErrors { get; set; } = new();
        public List<SanaTotalCassette> Cassettes { get; set; } = new();
    }

    public sealed class SanaTotalCassette
    {
        public int Id { get; set; }
        public long Denomination { get; set; }
        public int InitialCount { get; set; }
        public int TotalPickup { get; set; }
        public int TotalDispense { get; set; }
        public int TotalReject { get; set; }
        public int LastKnownCount { get; set; }
    }

    public sealed class SanaTotalError
    {
        public string? ErrorCode { get; set; }
        public string? Description { get; set; }
        public string? Device { get; set; }
        public int Count { get; set; }
        public List<string> Dates { get; set; } = new();
    }

    // ---------- مدل‌های پاسخ API ----------

    public sealed record TotalUploadResponse(
        bool Success,
        string Message,
        Guid? ReportId = null,
        string? TicketNumber = null,
        string? SerialNumber = null,
        string? FirstLogDate = null,
        string? LastLogDate = null,
        int DailyFileCount = 0,
        bool Replaced = false);

    public sealed record TotalArchiveItem(
        Guid ReportId,
        string TicketNumber,
        string SerialNumber,
        string FirstLogDate,
        string LastLogDate,
        string? ExportDate,
        string UploadedAt,
        long FileSizeBytes,
        int DailyFileCount,
        string DownloadUrl);

    public sealed record TotalArchiveListResponse(
        int TotalCount,
        int Page,
        int PageSize,
        List<TotalArchiveItem> Items);

    /// <summary>وضعیت وجود گزارش برای یک شماره تیکت.</summary>
    public sealed record TicketStatusResponse(
        string TicketNumber,
        TicketDataStatus Status,
        string StatusText,
        bool HasDaily,
        bool HasTotal,
        bool CanClose,
        TicketDailySummary? Daily,
        TicketTotalSummary? Total);

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TicketDataStatus
    {
        /// <summary>هیچ گزارشی ثبت نشده است.</summary>
        None = 0,

        /// <summary>فقط گزارش هفتگی (اسکن QR) موجود است.</summary>
        DailyOnly = 1,

        /// <summary>فقط گزارش توتال موجود است.</summary>
        TotalOnly = 2,

        /// <summary>هر دو گزارش موجود است.</summary>
        Both = 3
    }

    public sealed record TicketDailySummary(
        int ReportCount,
        string? FirstDate,
        string? LastDate,
        string? SerialNumber,
        int TotalTransactions,
        int ErrorCount);

    public sealed record TicketTotalSummary(
        Guid ReportId,
        string SerialNumber,
        string FirstLogDate,
        string LastLogDate,
        string UploadedAt,
        int TotalTransactions,
        int TotalCards,
        int DailyFileCount,
        string DownloadUrl);
}