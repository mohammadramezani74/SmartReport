namespace SmartReportLog.Model.Atm.Queries
{
    public class AtmErrorCountDto
    {
        // شناسه دستگاه برای لینک به صفحه جزئیات
        public Guid AtmId { get; set; }

        // نام/سریال دستگاه - روی محور X نمودار
        public string SerialNumber { get; set; } = string.Empty;

        // تعداد کل خطاها - ارتفاع ستون
        public int ErrorCount { get; set; }
    }
}
