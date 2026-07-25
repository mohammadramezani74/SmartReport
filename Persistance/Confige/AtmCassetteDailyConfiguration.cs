using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmCassetteDailyConfiguration
   : IEntityTypeConfiguration<AtmCassetteDaily>
    {
        public void Configure(EntityTypeBuilder<AtmCassetteDaily> builder)
        {


            builder.HasKey(x => x.Id);

        }
    }
}
