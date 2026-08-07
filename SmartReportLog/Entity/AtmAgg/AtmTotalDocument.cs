using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    /// <summary>
    /// محتوای خام فایل‌های JSON داخل آرشیو سانا.
    /// جدا از AtmTotalReport نگه داشته می‌شود تا کوئری‌های لیست و آمار
    /// مجبور به خواندن چند ده کیلوبایت متن نباشند.
    /// </summary>
    public sealed class AtmTotalDocument : Entity<Guid>
    {
        public Guid TotalReportId { get; private set; }

        /// <summary>محتوای خام total.json</summary>
        public string TotalJson { get; private set; } = default!;

        /// <summary>محتوای خام info.json — در فرمت قدیمی وجود ندارد.</summary>
        public string? InfoJson { get; private set; }

        /// <summary>محتوای خام config.json</summary>
        public string? ConfigJson { get; private set; }

        /// <summary>محتوای خام denomination.json</summary>
        public string? DenominationJson { get; private set; }

        private AtmTotalDocument() { }

        public static AtmTotalDocument Create(
            Guid totalReportId, string totalJson,
            string? infoJson, string? configJson, string? denominationJson) => new()
            {
                Id = Guid.NewGuid(),
                TotalReportId = totalReportId,
                TotalJson = totalJson,
                InfoJson = infoJson,
                ConfigJson = configJson,
                DenominationJson = denominationJson,
                CreateDate = DateTime.Now
            };

        public void Edit(string totalJson, string? infoJson,
            string? configJson, string? denominationJson)
        {
            TotalJson = totalJson;
            InfoJson = infoJson;
            ConfigJson = configJson;
            DenominationJson = denominationJson;
            ModifiedDate = DateTime.Now;
        }
    }
}