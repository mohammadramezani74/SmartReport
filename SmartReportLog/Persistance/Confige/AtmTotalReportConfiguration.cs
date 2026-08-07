using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{
    public sealed class AtmTotalReportConfiguration : IEntityTypeConfiguration<AtmTotalReport>
    {
        public void Configure(EntityTypeBuilder<AtmTotalReport> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SerialNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TicketNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.BankName).HasMaxLength(150);
            builder.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            builder.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            builder.Property(x => x.FileHash).HasMaxLength(64).IsRequired();

            builder.HasIndex(x => x.TicketNumber);
            builder.HasIndex(x => x.SerialNumber);
            builder.HasIndex(x => new { x.FirstLogDate, x.LastLogDate });

            // یک تیکت + یک بازه فقط یک بار ثبت می‌شود؛ آپلود مجدد جایگزین می‌شود
            builder.HasIndex(x => new { x.TicketNumber, x.FirstLogDate, x.LastLogDate })
                   .IsUnique();

            builder.HasOne<Atm>()
                   .WithMany()
                   .HasForeignKey(x => x.AtmId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Cassettes)
                   .WithOne()
                   .HasForeignKey(x => x.TotalReportId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Errors)
                   .WithOne()
                   .HasForeignKey(x => x.TotalReportId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Document)
       .WithOne()
       .HasForeignKey<AtmTotalDocument>(x => x.TotalReportId)
       .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class AtmTotalCassetteConfiguration : IEntityTypeConfiguration<AtmTotalCassette>
    {
        public void Configure(EntityTypeBuilder<AtmTotalCassette> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TotalReportId, x.CassetteId }).IsUnique();
        }
    }

    public sealed class AtmTotalErrorConfiguration : IEntityTypeConfiguration<AtmTotalError>
    {
        public void Configure(EntityTypeBuilder<AtmTotalError> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Device).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ErrorCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.TotalReportId);
            builder.HasIndex(x => new { x.Device, x.ErrorCode });

            builder.HasMany(x => x.Dates)
                   .WithOne()
                   .HasForeignKey(x => x.TotalErrorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class AtmTotalErrorDateConfiguration : IEntityTypeConfiguration<AtmTotalErrorDate>
    {
        public void Configure(EntityTypeBuilder<AtmTotalErrorDate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TotalErrorId, x.Date }).IsUnique();
        }
    }
}