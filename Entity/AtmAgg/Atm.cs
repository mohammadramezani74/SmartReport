using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class Atm : Entity<Guid>
    {
        public string? SerialNumber { get; private set; }
        public string? CpuModel { get; private set; }
        public string? OsVersion { get; private set; }

        public ICollection<AtmDailyAnalysis> DailyAnalyses { get; private set; } = new List<AtmDailyAnalysis>();
        private Atm() { } // برای EF Core

        public static Atm Create(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("سریال ATM نامعتبر است.");

            return new Atm { Id = Guid.NewGuid(), SerialNumber = serialNumber };
        }
        public void UpdateHardwareInfo(string? cpuModel, string? osVersion)
        {
            if (!string.IsNullOrWhiteSpace(cpuModel)) CpuModel = cpuModel;
            if (!string.IsNullOrWhiteSpace(osVersion)) OsVersion = osVersion;
        }

        public AtmDailyAnalysis UpsertDailyAnalysis(
          DateOnly date, DateOnly endDate, int totalCards, int totalTransactions, int receiptCount,
          double auiSeconds, int dispenseTotal, int rejectTotal,
          IEnumerable<(int Id, long Denomination, int Pickup, int Dispense, int Reject, int LastKnown)> cassettes,
          IEnumerable<(string Device, string ErrorCode, int Count)> errors,
          IEnumerable<(string Device, string ErrorCode, int Count, DateOnly Date)> todayErrors,
          int cpuUsagePercent, int ramTotalGb, int ramUsedGb, int? cpuTemperatureC,
          int diskTotalGb, int diskUsedGb, string? ver)
        {
            var existing = DailyAnalyses.FirstOrDefault(d => d.Date == date && d.EndDate == endDate);
            if (existing != null)
            {
                existing.Update(totalCards, totalTransactions, receiptCount, auiSeconds,
                    dispenseTotal, rejectTotal, cassettes, errors, todayErrors, cpuUsagePercent, ramTotalGb, ramUsedGb, cpuTemperatureC, diskTotalGb, diskUsedGb, ver);
                return existing;
            }
            var analysis = AtmDailyAnalysis.Create(Id, date, endDate, totalCards, totalTransactions,
                receiptCount, auiSeconds, dispenseTotal, rejectTotal, cassettes, errors, todayErrors,
                cpuUsagePercent, ramTotalGb, ramUsedGb, cpuTemperatureC, diskTotalGb, diskUsedGb, ver);
            DailyAnalyses.Add(analysis);
            return analysis;
        }
    }
}
