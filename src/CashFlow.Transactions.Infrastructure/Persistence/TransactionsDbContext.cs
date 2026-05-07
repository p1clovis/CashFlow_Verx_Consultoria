using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Transactions.Infrastructure.Persistence;

public class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(500).IsRequired();
            entity.Property(t => t.Type).HasConversion<string>().IsRequired();
            entity.Property(t => t.OccurredOn).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();

            entity.HasIndex(t => t.OccurredOn);  // optimize daily queries
        });
    }
}
