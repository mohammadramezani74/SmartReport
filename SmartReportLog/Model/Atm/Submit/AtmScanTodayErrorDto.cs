namespace SmartReportLog.Model.Atm.Submit
{
    public sealed class AtmScanTodayErrorDto
    {
        public string D { get; set; } = default!;  // Device
        public string E { get; set; } = default!;  // ErrorCode
        public int C { get; set; }                 // Count
        public string Dt { get; set; } = default!;  // Date (yyyy-MM-dd)
    }
}
