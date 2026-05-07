using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Interfaces;
using CashFlow.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Transactions.Infrastructure.Repositories;

public class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default)
    {
        await context.Transactions.AddAsync(transaction, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Transaction>> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var start = date.Date;
        var end   = start.AddDays(1);

        return await context.Transactions
            .Where(t => t.OccurredOn >= start && t.OccurredOn < end)
            .OrderBy(t => t.OccurredOn)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
        => await context.Transactions
            .OrderByDescending(t => t.OccurredOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
}
