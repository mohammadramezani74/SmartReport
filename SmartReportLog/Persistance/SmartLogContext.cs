using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;
using SmartReportLog.Entity.Identity;

namespace SmartReportLog.Persistance
{
    public class SmartLogContext :IdentityDbContext<AppUser>
    {
        public SmartLogContext(
            DbContextOptions<SmartLogContext> options)
            : base(options)
        {
        }

        public DbSet<Atm> Atms => Set<Atm>();

        public DbSet<AtmDailyAnalysis> DailyAnalyses => Set<AtmDailyAnalysis>();

        public DbSet<AtmCassetteDaily> DailyCassettes => Set<AtmCassetteDaily>();

        public DbSet<AtmHardwareErrorDaily> DailyHardwareErrors => Set<AtmHardwareErrorDaily>();
        public DbSet<AtmTodayError> AtmTodayErrors => Set<AtmTodayError>();
        public DbSet<JsonDocuments> jsonDocuments => Set<JsonDocuments>();
        public DbSet<AtmTicketInfo> AtmTicketInfos => Set<AtmTicketInfo>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLogContext).Assembly);

        }
    }
}
