using SmartReportLog.Model.Sana;

namespace SmartReportLog.Services.Sana
{
    public interface ISanaArchiveService
    {
        Task<TotalUploadResponse> IngestAsync(
            Stream zipStream, string originalFileName, CancellationToken ct);

        Task<TotalArchiveListResponse> GetArchivesAsync(
            string? ticketNumber, string? serialNumber,
            DateOnly? from, DateOnly? to,
            int page, int pageSize, CancellationToken ct);

        Task<(Stream? Stream, string? FileName)> OpenArchiveAsync(
            Guid reportId, CancellationToken ct);

        Task<TicketStatusResponse> GetTicketStatusAsync(
            string ticketNumber, CancellationToken ct);
    }

    public sealed class SanaStorageOptions
    {
        public const string SectionName = "SanaStorage";

        /// <summary>پوشه ذخیره فایل‌های زیپ دریافتی.</summary>
        public string RootPath { get; set; } = @"D:\SmartReportArchive\SanaTotal";

        /// <summary>حداکثر حجم مجاز فایل بر حسب بایت.</summary>
        public long MaxFileSizeBytes { get; set; } = 200L * 1024 * 1024;

        /// <summary>کلیدهای معتبر برای دسترسی به APIها.</summary>
        public List<ApiClientKey> ApiKeys { get; set; } = new();
    }

    public sealed class ApiClientKey
    {
        public string Name { get; set; } = default!;
        public string Key { get; set; } = default!;

        /// <summary>upload | read</summary>
        public List<string> Scopes { get; set; } = new();
    }
}