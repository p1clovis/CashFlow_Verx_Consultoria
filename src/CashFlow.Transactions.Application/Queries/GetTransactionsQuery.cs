using CashFlow.Transactions.Domain.Interfaces;
using MediatR;

namespace CashFlow.Transactions.Application.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public record GetTransactionsQuery(int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<TransactionDto>>;

public record TransactionDto(Guid Id, decimal Amount, string Type, string Description, DateTime OccurredOn, DateTime CreatedAt);

// ── Handler ───────────────────────────────────────────────────────────────────

public class GetTransactionsQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken ct)
    {
        var transactions = await repository.GetAllAsync(request.Page, request.PageSize, ct);
        return transactions.Select(t => new TransactionDto(
            t.Id, t.Amount, t.Type.ToString(), t.Description, t.OccurredOn, t.CreatedAt)).ToList();
    }
}

// ── By Date ───────────────────────────────────────────────────────────────────

public record GetTransactionsByDateQuery(DateTime Date) : IRequest<IReadOnlyList<TransactionDto>>;

public class GetTransactionsByDateQueryHandler(ITransactionRepository repository)
    : IRequestHandler<GetTransactionsByDateQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionsByDateQuery request, CancellationToken ct)
    {
        var transactions = await repository.GetByDateAsync(request.Date, ct);
        return transactions.Select(t => new TransactionDto(
            t.Id, t.Amount, t.Type.ToString(), t.Description, t.OccurredOn, t.CreatedAt)).ToList();
    }
}
