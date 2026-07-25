using SmartReportLog.Entity.Common;

namespace SmartReportLog.Entity.AtmAgg
{
    public class JsonDocuments : Entity<Guid>
    {
        public string? SerialNumber { get; private set; }

        public string? JsonContent { get; private set; }

        public DateOnly StartDate { get; private set; }

        public DateOnly EndDate { get; private set; }

        public static JsonDocuments Create(DateOnly startdate,DateOnly enddate,
            string json,string SerialNo
            ) => new JsonDocuments
        {
            Id = Guid.NewGuid(),
            CreateDate=DateTime.Now,
            StartDate=startdate,
            EndDate=enddate,
            JsonContent = json
             ,SerialNumber = SerialNo 
            
        };
        public void Edit(string json)
        {
            JsonContent = json;
            ModifiedDate = DateTime.Now;
        }
    }
}
