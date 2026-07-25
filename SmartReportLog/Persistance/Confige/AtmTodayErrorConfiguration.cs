using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmTodayErrorConfiguration
    : IEntityTypeConfiguration<AtmTodayError>
    {
        public void Configure(EntityTypeBuilder<AtmTodayError> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Device)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ErrorCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Count)
                .IsRequired();

            builder.Property(x => x.Date)
                .IsRequired();

            builder.HasIndex(x => new
            {
                x.DailyAnalysisId,
                x.Date,
                x.Device,
                x.ErrorCode
            });
        }
    }
}
