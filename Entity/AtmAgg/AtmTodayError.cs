using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmTodayError : Entity<Guid>
    {
        public Guid DailyAnalysisId { get; private set; }
        public string Device { get; private set; }
        public string ErrorCode { get; private set; }
        public int Count { get; private set; }
        public DateOnly Date { get; private set; }

        private AtmTodayError() { }

        public static AtmTodayError Create(Guid dailyAnalysisId, string device, string errorCode, int count, DateOnly date) => new()
        {
            Id = Guid.NewGuid(),
            DailyAnalysisId = dailyAnalysisId,
            Device = device,
            ErrorCode = errorCode,
            Count = count,
            Date = date
        };
    }
}
