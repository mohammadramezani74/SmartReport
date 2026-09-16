namespace SmartReportLog.Model.Atm.Submit
{
    public sealed class AtmScanDayDto
    {
 
        public string Tk { get; set; }         //TicketNumber
        public string Pc { get; set; }         //personnelCode
        public string S { get; set; } = default!;      // SerialNumber
        public string Dt { get; set; } = default!;      // Date (yyyy-MM-dd)
        public int Ca { get; set; }                      // TotalCards
        public int Tr { get; set; }                      // TotalTransactions
        public int Re { get; set; }                      // ReceiptCount
        public double Au { get; set; }                    // AuiSeconds
        public int Di { get; set; }                      // DailyDispenseTotal
        public int Rj { get; set; }                      // DailyRejectTotal
        public List<AtmScanErrorDto> Er { get; set; } = new();
        public List<AtmScanCassetteDto> Cs { get; set; } = new();
        public List<AtmScanTodayErrorDto> TodayEr { get; set; } = new();
        public string? Cm { get; set; }   // CpuModel
        public string? Os { get; set; }   // WindowsVersion
        public string? Ver { get; set; } //version gaya
        public string? Iv { get; set; }   // ImageVersion
        public int Cu { get; set; }       // CpuUsagePercent
        public int Rt { get; set; }       // RamTotalGb
        public int Ru { get; set; }       // RamUsedGb
        public int Ct { get; set; } = -1; // CpuTemperature, -1 = نامشخص
        public int Dst { get; set; }      // DiskTotalGb
        public int Du { get; set; }       // DiskUsedGb
    }
}
