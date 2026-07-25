using Microsoft.AspNetCore.Identity;
using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class AtmDailyAnalysis : Entity<Guid>
    {
        public Guid AtmId { get;  set; }

        public DateOnly Date { get;  set; }
        public DateOnly EndDate { get; set; }

        public int TotalCards { get;  set; }

        public int TotalTransactions { get;  set; }

        public int ReceiptCount { get;  set; }

        public double AuiSeconds { get;  set; }

        public int DailyDispenseTotal { get;  set; }

        public int DailyRejectTotal { get;  set; }


        public ICollection<AtmCassetteDaily> Cassettes { get;  set; } = new List<AtmCassetteDaily>();

        public ICollection<AtmHardwareErrorDaily> HardwareErrors { get;  set; }=new List<AtmHardwareErrorDaily>();
        public ICollection<AtmTodayError> TodayErrors { get; set; } = new List<AtmTodayError>();
        public int CpuUsagePercent { get;  set; }
        public int RamTotalGb { get;  set; }
        public int RamUsedGb { get;  set; }
        public int? CpuTemperatureC { get;  set; } // null یعنی نامشخص/در دسترس نبود
        public int DiskTotalGb { get;  set; }
        public int DiskUsedGb { get;  set; }
        public string? GayaVersion { get; set; }
        private AtmDailyAnalysis() { }

        public static AtmDailyAnalysis Create(
            Guid atmId, DateOnly date,DateOnly endDate, int totalCards, int totalTransactions, int receiptCount,
            double auiSeconds, int dispenseTotal, int rejectTotal,
            IEnumerable<(int Id, long Denomination, int Pickup, int Dispense, int Reject, int LastKnown)> cassettes,
            IEnumerable<(string Device, string ErrorCode, int Count)> errors,
              IEnumerable<(string Device, string ErrorCode, int Count, DateOnly Date)> todayErrors,
             int cpuUsagePercent, int ramTotalGb, int ramUsedGb, int? cpuTemperatureC,
        int diskTotalGb, int diskUsedGb, string?ver
            )
        {
            var analysis = new AtmDailyAnalysis
            {
                Id = Guid.NewGuid(),
                AtmId = atmId,
                Date = date,
                EndDate = endDate,
               CreateDate=DateTime.Now,
            };
            analysis.Update(totalCards, totalTransactions, receiptCount, auiSeconds,
                dispenseTotal, rejectTotal, cassettes, errors, todayErrors, cpuUsagePercent, ramTotalGb, ramUsedGb, cpuTemperatureC, diskTotalGb, diskUsedGb,ver);
            return analysis;
        }

        public void Update(int totalCards, int totalTransactions, int receiptCount,
            double auiSeconds, int dispenseTotal, int rejectTotal,
            IEnumerable<(int Id, long Denomination, int Pickup, int Dispense, int Reject, int LastKnown)> cassettes,
            IEnumerable<(string Device, string ErrorCode, int Count)> errors,
            IEnumerable<(string Device, string ErrorCode, int Count, DateOnly Date)> todayErrors,
             int cpuUsagePercent, int ramTotalGb, int ramUsedGb, int? cpuTemperatureC,
        int diskTotalGb, int diskUsedGb,string? ver)
        {
            TotalCards = totalCards;
            TotalTransactions = totalTransactions;
            ReceiptCount = receiptCount;
            AuiSeconds = auiSeconds;
            DailyDispenseTotal = dispenseTotal;
            DailyRejectTotal = rejectTotal;
            ModifiedDate = DateTime.Now;
            CpuUsagePercent = cpuUsagePercent;
            RamTotalGb = ramTotalGb;
            RamUsedGb = ramUsedGb;
            CpuTemperatureC = (cpuTemperatureC.HasValue && cpuTemperatureC.Value >= 0) ? cpuTemperatureC : null;
            DiskTotalGb = diskTotalGb;
            DiskUsedGb = diskUsedGb;
            GayaVersion = ver;

            foreach (var c in cassettes)
                Cassettes.Add(AtmCassetteDaily.Create(Id, c.Id, c.Denomination, c.Pickup, c.Dispense, c.Reject, c.LastKnown));

            foreach (var e in errors)
                HardwareErrors.Add(AtmHardwareErrorDaily.Create(Id, e.Device, e.ErrorCode, e.Count));

            foreach (var t in todayErrors)
            {
                bool alreadyExists = TodayErrors.Any(x =>
                    x.Device == t.Device && x.ErrorCode == t.ErrorCode && x.Date == t.Date);

                if (!alreadyExists)
                    TodayErrors.Add(AtmTodayError.Create(Id, t.Device, t.ErrorCode, t.Count, t.Date));
            }
        }
    }
}
