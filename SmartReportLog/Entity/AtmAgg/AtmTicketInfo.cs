using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmTicketInfo : Entity<Guid>
    {
        public Guid DailyAnalysisId { get; private set; }

        public int RequestNo { get; private set; }
        public string? CallDate { get; private set; }
        public string? ReferDate { get; private set; }
        public string? ReferEndTime { get; private set; }
        public string? AssignType { get; private set; }
        public string? TechName { get; private set; }

        public DateTime FetchedAt { get; private set; }

        private AtmTicketInfo() { }

        public static AtmTicketInfo Create(Guid dailyAnalysisId, int requestNo,
            string? callDate, string? referDate, string? referEndTime,
            string? assignType, string? techName) => new()
            {
                Id = Guid.NewGuid(),
                DailyAnalysisId = dailyAnalysisId,
                RequestNo = requestNo,
                CallDate = callDate,
                ReferDate = referDate,
                ReferEndTime = referEndTime,
                AssignType = assignType,
                TechName = techName,
                FetchedAt = DateTime.Now,
                CreateDate = DateTime.Now
            };

        public void Refresh(string? callDate, string? referDate, string? referEndTime,
            string? assignType, string? techName)
        {
            CallDate = callDate;
            ReferDate = referDate;
            ReferEndTime = referEndTime;
            AssignType = assignType;
            TechName = techName;
            FetchedAt = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}