using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmHardwareErrorDaily : Entity<Guid>
    {
        public Guid DailyAnalysisId { get; private set; }

        public string Device { get; private set; }

        public string ErrorCode { get; private set; }

        public string Description { get; private set; }

        public int Count { get; private set; }
        public static AtmHardwareErrorDaily Create(Guid dailyAnalysisId, string device, string errorCode, int count) => new()
        {
            Id = Guid.NewGuid(),
            DailyAnalysisId = dailyAnalysisId,
            Device = device,
            ErrorCode = errorCode,
            Count = count
            ,Description=device
        };
    }
}
