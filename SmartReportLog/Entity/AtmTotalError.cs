using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmTotalError : Entity<Guid>
    {
        public Guid TotalReportId { get; private set; }
        public string Device { get; private set; } = default!;
        public string ErrorCode { get; private set; } = default!;
        public string? Description { get; private set; }
        public int Count { get; private set; }

        public ICollection<AtmTotalErrorDate> Dates { get; private set; } = new List<AtmTotalErrorDate>();

        private AtmTotalError() { }

        public static AtmTotalError Create(
            Guid totalReportId, string device, string errorCode,
            string? description, int count) => new()
            {
                Id = Guid.NewGuid(),
                TotalReportId = totalReportId,
                Device = device,
                ErrorCode = errorCode,
                Description = description,
                Count = count,
                CreateDate = DateTime.Now
            };

        public void AddDate(DateOnly date, int count) =>
            Dates.Add(AtmTotalErrorDate.Create(Id, date, count));
    }
}