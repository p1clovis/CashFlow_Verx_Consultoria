using CashFlow.Consolidation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Persistence;

public class ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options) : DbContext(options)
{
    public DbSet<DailyBalance> DailyBalances => Set<DailyBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyBalance>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.TotalCredits).HasPrecision(18, 2);
            entity.Property(b => b.TotalDebits).HasPrecision(18, 2);
            entity.HasIndex(b => b.Date).IsUnique();  // one row per day
        });
    }
}
