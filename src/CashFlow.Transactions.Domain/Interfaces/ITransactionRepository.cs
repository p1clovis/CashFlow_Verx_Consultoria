using CashFlow.Transactions.Domain.Entities;

namespace CashFlow.Transactions.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
}
