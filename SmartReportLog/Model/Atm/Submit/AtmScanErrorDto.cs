namespace SmartReportLog.Model.Atm.Submit
{
    public sealed class AtmScanErrorDto
    {
        public string D { get; set; } = default!;   // Device
        public string E { get; set; } = default!;   // ErrorCode
        public int C { get; set; }                  // Count
    }
}
