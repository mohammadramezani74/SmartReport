using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmTotalReport : Entity<Guid>
    {
        public Guid AtmId { get; private set; }

        // ---------- شناسه‌ها ----------
        public string SerialNumber { get; private set; } = default!;
        public string TicketNumber { get; private set; } = default!;

        // ---------- بازه ----------
        public DateOnly FirstLogDate { get; private set; }
        public DateOnly LastLogDate { get; private set; }
        public DateTime? ExportDate { get; private set; }

        // ---------- آمار کلی ----------
        public int TotalCards { get; private set; }
        public int TotalTransactions { get; private set; }
        public int TotalReceipts { get; private set; }
        public int TotalReject { get; private set; }

        /// <summary>مقدار خام TotalDispense در فایل — گاهی صفر گزارش می‌شود.</summary>
        public int TotalDispenseRaw { get; private set; }

        /// <summary>مجموع TotalDispense کاست‌ها — مقدار قابل اتکا برای گزارش‌گیری.</summary>
        public int TotalDispenseComputed { get; private set; }

        public string? BankName { get; private set; }

        // ---------- فایل آرشیو ----------
        public string StoredFileName { get; private set; } = default!;
        public string OriginalFileName { get; private set; } = default!;
        public long FileSizeBytes { get; private set; }
        public string FileHash { get; private set; } = default!;
        public int DailyFileCount { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public AtmTotalDocument? Document { get; set; }
        public ICollection<AtmTotalCassette> Cassettes { get; private set; } = new List<AtmTotalCassette>();
        public ICollection<AtmTotalError> Errors { get; private set; } = new List<AtmTotalError>();

        private AtmTotalReport() { }

        public static AtmTotalReport Create(
            Guid atmId, string serialNumber, string ticketNumber,
            DateOnly firstLogDate, DateOnly lastLogDate, DateTime? exportDate,
            int totalCards, int totalTransactions, int totalReceipts,
            int totalReject, int totalDispenseRaw, int totalDispenseComputed,
            string? bankName,
            string storedFileName, string originalFileName,
            long fileSizeBytes, string fileHash, int dailyFileCount) => new()
            {
                Id = Guid.NewGuid(),
                AtmId = atmId,
                SerialNumber = serialNumber,
                TicketNumber = ticketNumber,
                FirstLogDate = firstLogDate,
                LastLogDate = lastLogDate,
                ExportDate = exportDate,
                TotalCards = totalCards,
                TotalTransactions = totalTransactions,
                TotalReceipts = totalReceipts,
                TotalReject = totalReject,
                TotalDispenseRaw = totalDispenseRaw,
                TotalDispenseComputed = totalDispenseComputed,
                BankName = bankName,
                StoredFileName = storedFileName,
                OriginalFileName = originalFileName,
                FileSizeBytes = fileSizeBytes,
                FileHash = fileHash,
                DailyFileCount = dailyFileCount,
                UploadedAt = DateTime.Now,
                CreateDate = DateTime.Now
            };

        public void AddCassette(AtmTotalCassette cassette) => Cassettes.Add(cassette);

        public void AddError(AtmTotalError error) => Errors.Add(error);
    }
}