using CashFlow.Transactions.Application.Interfaces;
using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Enums;
using CashFlow.Transactions.Domain.Interfaces;
using CashFlow.Shared.Events;
using MediatR;

namespace CashFlow.Transactions.Application.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public record CreateTransactionCommand(
    decimal Amount,
    string Type,           // "Credit" | "Debit"
    string Description,
    DateTime? OccurredOn
) : IRequest<CreateTransactionResult>;

public record CreateTransactionResult(Guid Id, decimal Amount, string Type, DateTime OccurredOn);

// ── Handler ───────────────────────────────────────────────────────────────────

public class CreateTransactionCommandHandler(
    ITransactionRepository repository,
    IEventPublisher publisher)
    : IRequestHandler<CreateTransactionCommand, CreateTransactionResult>
{
    public async Task<CreateTransactionResult> Handle(
        CreateTransactionCommand command,
        CancellationToken ct)
    {
        if (!Enum.TryParse<TransactionType>(command.Type, ignoreCase: true, out var type))
            throw new ArgumentException($"Invalid transaction type: '{command.Type}'. Use 'Credit' or 'Debit'.");

        var transaction = Transaction.Create(
            command.Amount,
            type,
            command.Description,
            command.OccurredOn);

        await repository.AddAsync(transaction, ct);

        // Publish event — fire and forget; consolidation service listens async
        var @event = new TransactionCreatedEvent(
            transaction.Id,
            transaction.Amount,
            transaction.Type.ToString(),
            transaction.Description,
            transaction.OccurredOn);

        await publisher.PublishAsync(@event, ct);

        return new CreateTransactionResult(
            transaction.Id,
            transaction.Amount,
            transaction.Type.ToString(),
            transaction.OccurredOn);
    }
}
