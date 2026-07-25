using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance.Confige
{

    public sealed partial class AtmConfiguration
: IEntityTypeConfiguration<Atm>
    {
        public void Configure(EntityTypeBuilder<Atm> builder)
        {


            builder.HasKey(x => x.Id);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(50)
                .IsRequired();
            builder.Property(x => x.OsVersion)
              .HasMaxLength(150);
            builder.Property(x => x.CpuModel)
           .HasMaxLength(150);

            builder.HasIndex(x => x.SerialNumber)
                .IsUnique();

            builder.HasMany(x => x.DailyAnalyses)
                .WithOne()
                .HasForeignKey(x => x.AtmId);


        }
    }
}
