using CashFlow.Consolidation.Domain.Entities;

namespace CashFlow.Consolidation.Domain.Interfaces;

public interface IDailyBalanceRepository
{
    Task<DailyBalance?> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<IReadOnlyList<DailyBalance>> GetRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task AddAsync(DailyBalance balance, CancellationToken ct = default);
    Task UpdateAsync(DailyBalance balance, CancellationToken ct = default);
}
