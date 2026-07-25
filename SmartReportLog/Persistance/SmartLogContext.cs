using Microsoft.EntityFrameworkCore;
using SmartReportLog.Entity.AtmAgg;

namespace SmartReportLog.Persistance
{
    public class SmartLogContext : DbContext
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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLogContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
