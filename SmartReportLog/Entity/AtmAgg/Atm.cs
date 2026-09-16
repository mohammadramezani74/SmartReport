using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public sealed class Atm : Entity<Guid>
    {
        public string? SerialNumber { get; private set; }
        public string? CpuModel { get; private set; }
        public string? OsVersion { get; private set; }
        public string? MInvCode { get; private set; }
        public string? DeviceName { get; private set; }
        public int? StateCode { get; private set; }
        public string? StateName { get; private set; }
        public string? CityName { get; private set; }
        public string? SupervisionStateName { get; private set; }
        public string? CustomerName { get; private set; }
        public string? BranchCode { get; private set; }
        public string? BranchName { get; private set; }


        public ICollection<AtmDailyAnalysis> DailyAnalyses { get; private set; } = new List<AtmDailyAnalysis>();
        private Atm() { } // برای EF Core

        public static Atm Create(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
                throw new ArgumentException("سریال ATM نامعتبر است.");

            return new Atm { Id = Guid.NewGuid(), SerialNumber = serialNumber ,CreateDate=DateTime.Now};
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
          int diskTotalGb, int diskUsedGb, string? ver,string? imageversion,
    string? ticketNumber, string? personnelCode)
        {
            var existing = DailyAnalyses.FirstOrDefault(d => d.Date == date && d.EndDate == endDate);
            if (existing != null)
            {
                existing.Update(totalCards, totalTransactions, receiptCount, auiSeconds,
                    dispenseTotal, rejectTotal, cassettes, errors, todayErrors
                    , cpuUsagePercent, ramTotalGb, ramUsedGb, cpuTemperatureC,
                    diskTotalGb, diskUsedGb, ver,imageversion, ticketNumber, personnelCode);
                return existing;
            }
            var analysis = AtmDailyAnalysis.Create(Id, date, endDate, totalCards, totalTransactions,
                receiptCount, auiSeconds, dispenseTotal, rejectTotal, cassettes, errors, todayErrors,
                cpuUsagePercent, ramTotalGb, ramUsedGb, cpuTemperatureC, diskTotalGb, diskUsedGb, ver,imageversion, ticketNumber, personnelCode);
            DailyAnalyses.Add(analysis);
            return analysis;
        }

        public void UpdateLocationInfo(string? mInvCode, string? deviceName, int? stateCode,
    string? stateName, string? cityName, string? supervisionStateName,
    string? customerName, string? branchCode, string? branchName)
        {
            if (!string.IsNullOrWhiteSpace(mInvCode)) MInvCode = mInvCode;
            if (!string.IsNullOrWhiteSpace(deviceName)) DeviceName = deviceName;
            if (stateCode.HasValue) StateCode = stateCode;
            if (!string.IsNullOrWhiteSpace(stateName)) StateName = stateName;
            if (!string.IsNullOrWhiteSpace(cityName)) CityName = cityName;
            if (!string.IsNullOrWhiteSpace(supervisionStateName)) SupervisionStateName = supervisionStateName;
            if (!string.IsNullOrWhiteSpace(customerName)) CustomerName = customerName;
            if (!string.IsNullOrWhiteSpace(branchCode)) BranchCode = branchCode;
            if (!string.IsNullOrWhiteSpace(branchName)) BranchName = branchName;
        }
    }
}
