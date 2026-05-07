using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Interfaces;
using CashFlow.Consolidation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Consolidation.Infrastructure.Repositories;

public class DailyBalanceRepository(ConsolidationDbContext context) : IDailyBalanceRepository
{
    public async Task<DailyBalance?> GetByDateAsync(DateTime date, CancellationToken ct = default)
        => await context.DailyBalances
            .FirstOrDefaultAsync(b => b.Date == date.Date, ct);

    public async Task<IReadOnlyList<DailyBalance>> GetRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await context.DailyBalances
            .Where(b => b.Date >= from.Date && b.Date <= to.Date)
            .OrderBy(b => b.Date)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task AddAsync(DailyBalance balance, CancellationToken ct = default)
    {
        await context.DailyBalances.AddAsync(balance, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DailyBalance balance, CancellationToken ct = default)
    {
        context.DailyBalances.Update(balance);
        await context.SaveChangesAsync(ct);
    }
}
