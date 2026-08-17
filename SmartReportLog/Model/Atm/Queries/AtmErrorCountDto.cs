namespace SmartReportLog.Model.Atm.Queries
{
    public class AtmErrorCountDto
    {
        public Guid AtmId { get; set; }
        public string SerialNumber { get; set; } = default!;
        public int ErrorCount { get; set; }
        public int ReportCount { get; set; }
        public DateOnly? FirstDate { get; set; }
        public DateOnly? LastDate { get; set; }
    }
}