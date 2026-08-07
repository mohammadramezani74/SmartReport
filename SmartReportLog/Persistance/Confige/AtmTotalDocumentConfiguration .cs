using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmTotalDocumentConfiguration : IEntityTypeConfiguration<AtmTotalDocument>
    {
        public void Configure(EntityTypeBuilder<AtmTotalDocument> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.TotalReportId).IsUnique();

            // nvarchar(max) — محتوای JSON محدودیت طول ندارد
            builder.Property(x => x.TotalJson).IsRequired();
        }
    }
}
