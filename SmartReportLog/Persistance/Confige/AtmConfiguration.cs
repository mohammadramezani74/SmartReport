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
            builder.Property(x => x.MInvCode).HasMaxLength(50);
            builder.Property(x => x.DeviceName).HasMaxLength(200);
            builder.Property(x => x.StateName).HasMaxLength(100);
            builder.Property(x => x.CityName).HasMaxLength(100);
            builder.Property(x => x.SupervisionStateName).HasMaxLength(100);
            builder.Property(x => x.CustomerName).HasMaxLength(200);
            builder.Property(x => x.BranchCode).HasMaxLength(50);
            builder.Property(x => x.BranchName).HasMaxLength(200);

            builder.HasIndex(x => x.StateCode);
            builder.HasIndex(x => x.BranchCode);


        }
    }
}
