using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    /// <summary>
    /// تاریخ‌های خطا به صورت تجمیع‌شده. آرایه Dates در فایل سانا تکراری است
    /// (به ازای هر رخداد یک عضو)، اینجا به (تاریخ، تعداد) خلاصه می‌شود.
    /// </summary>
    public sealed class AtmTotalErrorDate : Entity<Guid>
    {
        public Guid TotalErrorId { get; private set; }
        public DateOnly Date { get; private set; }
        public int Count { get; private set; }

        private AtmTotalErrorDate() { }

        public static AtmTotalErrorDate Create(Guid totalErrorId, DateOnly date, int count) => new()
        {
            Id = Guid.NewGuid(),
            TotalErrorId = totalErrorId,
            Date = date,
            Count = count,
            CreateDate = DateTime.Now
        };
    }
}