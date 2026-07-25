namespace SmartReportLog.Entity.Common
{
    public class Entity<T>
    {
        public T Id { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
