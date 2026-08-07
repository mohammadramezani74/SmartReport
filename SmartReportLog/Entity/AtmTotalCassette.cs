using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmTotalCassette : Entity<Guid>
    {
        public Guid TotalReportId { get; private set; }
        public int CassetteId { get; private set; }
        public long Denomination { get; private set; }
        public int InitialCount { get; private set; }
        public int TotalPickup { get; private set; }
        public int TotalDispense { get; private set; }
        public int TotalReject { get; private set; }
        public int LastKnownCount { get; private set; }

        private AtmTotalCassette() { }

        public static AtmTotalCassette Create(
            Guid totalReportId, int cassetteId, long denomination, int initialCount,
            int totalPickup, int totalDispense, int totalReject, int lastKnownCount) => new()
            {
                Id = Guid.NewGuid(),
                TotalReportId = totalReportId,
                CassetteId = cassetteId,
                Denomination = denomination,
                InitialCount = initialCount,
                TotalPickup = totalPickup,
                TotalDispense = totalDispense,
                TotalReject = totalReject,
                LastKnownCount = lastKnownCount,
                CreateDate = DateTime.Now
            };
    }
}