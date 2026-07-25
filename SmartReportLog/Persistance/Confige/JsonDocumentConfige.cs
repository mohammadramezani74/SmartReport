using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public class JsonDocumentConfige : IEntityTypeConfiguration<JsonDocuments>
    {
        public void Configure(EntityTypeBuilder<JsonDocuments> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
