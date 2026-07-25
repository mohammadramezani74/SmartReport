using Microsoft.EntityFrameworkCore;
using SmartReportLog.Model.Ticket;

namespace SmartReportLog.Persistance
{
    public sealed class ErDbContext : DbContext
    {
        public ErDbContext(DbContextOptions<ErDbContext> options) : base(options) { }

        public DbSet<TicketInfoRow> TicketInfos => Set<TicketInfoRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketInfoRow>().HasNoKey().ToView(null);
        }
    }
}
