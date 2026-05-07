using CashFlow.Transactions.Domain.Enums;
using CashFlow.Transactions.Domain.Exceptions;

namespace CashFlow.Transactions.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string Description { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { } // EF Core

    public static Transaction Create(decimal amount, TransactionType type, string description, DateTime? occurredOn = null)
    {
        if (amount <= 0)
            throw new TransactionDomainException("Deve ser maior que zero !");

        if (string.IsNullOrWhiteSpace(description))
            throw new TransactionDomainException("Descrição é requerida !");

        return new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Type = type,
            Description = description.Trim(),
            OccurredOn = occurredOn ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsCredit => Type == TransactionType.Credit;
    public bool IsDebit => Type == TransactionType.Debit;
}
