using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmHardwareErrorDailyConfiguration
: IEntityTypeConfiguration<AtmHardwareErrorDaily>
    {
        public void Configure(EntityTypeBuilder<AtmHardwareErrorDaily> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Device)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Count)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.DailyAnalysisId,
                x.Device,
                x.ErrorCode
            });
        }
    }
}
