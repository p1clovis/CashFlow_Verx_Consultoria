using CashFlow.Consolidation.Domain.Interfaces;
using MediatR;

namespace CashFlow.Consolidation.Application.Queries;

// ── Single-day query ──────────────────────────────────────────────────────────

public record GetDailyBalanceQuery(DateTime Date) : IRequest<DailyBalanceDto?>;

public record DailyBalanceDto(
    DateTime Date,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    int TransactionCount,
    DateTime LastUpdatedAt);

public class GetDailyBalanceQueryHandler(IDailyBalanceRepository repository)
    : IRequestHandler<GetDailyBalanceQuery, DailyBalanceDto?>
{
    public async Task<DailyBalanceDto?> Handle(GetDailyBalanceQuery request, CancellationToken ct)
    {
        var balance = await repository.GetByDateAsync(request.Date, ct);
        if (balance is null) return null;

        return new DailyBalanceDto(
            balance.Date,
            balance.TotalCredits,
            balance.TotalDebits,
            balance.Balance,
            balance.TransactionCount,
            balance.LastUpdatedAt);
    }
}

// ── Range query ───────────────────────────────────────────────────────────────

public record GetBalanceRangeQuery(DateTime From, DateTime To) : IRequest<IReadOnlyList<DailyBalanceDto>>;

public class GetBalanceRangeQueryHandler(IDailyBalanceRepository repository)
    : IRequestHandler<GetBalanceRangeQuery, IReadOnlyList<DailyBalanceDto>>
{
    public async Task<IReadOnlyList<DailyBalanceDto>> Handle(GetBalanceRangeQuery request, CancellationToken ct)
    {
        var balances = await repository.GetRangeAsync(request.From, request.To, ct);
        return balances.Select(b => new DailyBalanceDto(
            b.Date, b.TotalCredits, b.TotalDebits, b.Balance, b.TransactionCount, b.LastUpdatedAt))
            .ToList();
    }
}
