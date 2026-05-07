namespace CashFlow.Consolidation.Domain.Entities;

public class DailyBalance
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }         // date only (time truncated)
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal Balance => TotalCredits - TotalDebits;
    public int TransactionCount { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    private DailyBalance() { }

    public static DailyBalance Create(DateTime date)
        => new()
        {
            Id             = Guid.NewGuid(),
            Date           = date.Date,
            TotalCredits   = 0,
            TotalDebits    = 0,
            TransactionCount = 0,
            LastUpdatedAt  = DateTime.UtcNow
        };

    public void ApplyCredit(decimal amount)
    {
        TotalCredits      += amount;
        TransactionCount  += 1;
        LastUpdatedAt      = DateTime.UtcNow;
    }

    public void ApplyDebit(decimal amount)
    {
        TotalDebits       += amount;
        TransactionCount  += 1;
        LastUpdatedAt      = DateTime.UtcNow;
    }
}
