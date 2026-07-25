using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmCassetteDaily : Entity<Guid>
    {

        public Guid DailyAnalysisId { get; private set; }


        public int CassetteNumber { get; private set; }

        public long Denomination { get; private set; }

        public int InitialCount { get; private set; }

        public int FinalCount { get; private set; }

        public int TotalPickup { get; private set; }

        public int TotalDispense { get; private set; }

        public int TotalReject { get; private set; }
        public static AtmCassetteDaily Create(Guid dailyAnalysisId, int index, long denomination,
       int pickup, int dispense, int reject, int lastKnown) => new()
       {
           Id = Guid.NewGuid(),
           DailyAnalysisId = dailyAnalysisId,
           CassetteNumber = index,
           Denomination = denomination,
           TotalPickup = pickup,
           TotalDispense = dispense,
           TotalReject = reject,

       };
    }
}
