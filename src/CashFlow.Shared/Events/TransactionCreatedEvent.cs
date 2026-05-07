namespace CashFlow.Shared.Events;

public record TransactionCreatedEvent(
    Guid TransactionId,
    decimal Amount,
    string Type,       // "Credit" or "Debit"
    string Description,
    DateTime OccurredOn
);
