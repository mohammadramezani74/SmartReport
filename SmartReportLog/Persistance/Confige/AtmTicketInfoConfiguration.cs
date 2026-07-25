using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmTicketInfoConfiguration
        : IEntityTypeConfiguration<AtmTicketInfo>
    {
        public void Configure(EntityTypeBuilder<AtmTicketInfo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.DailyAnalysisId).IsUnique();
            builder.HasIndex(x => x.RequestNo);

            builder.Property(x => x.CallDate).HasMaxLength(10);
            builder.Property(x => x.ReferDate).HasMaxLength(10);
            builder.Property(x => x.ReferEndTime).HasMaxLength(10);
            builder.Property(x => x.AssignType).HasMaxLength(100);
            builder.Property(x => x.TechName).HasMaxLength(150);
        }
    }
}
