using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmDailyAnalysisConfiguration
    : IEntityTypeConfiguration<AtmDailyAnalysis>
    {
        public void Configure(EntityTypeBuilder<AtmDailyAnalysis> builder)
        {


            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new
            {
                x.AtmId,
                x.Date
            }).IsUnique();

            builder.Property(x => x.AuiSeconds);

            builder.HasMany(x => x.Cassettes)
                .WithOne()
                .HasForeignKey(x => x.DailyAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.HardwareErrors)
                .WithOne()
                .HasForeignKey(x => x.DailyAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.TodayErrors)
    .WithOne()
    .HasForeignKey(x => x.DailyAnalysisId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
