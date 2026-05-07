using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Interfaces;
using CashFlow.Shared.Events;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.Events;

/// <summary>
/// Applies a TransactionCreatedEvent to the DailyBalance aggregate.
/// Called by the RabbitMQ consumer background service.
/// </summary>
public class TransactionCreatedEventHandler(
    IDailyBalanceRepository repository,
    ILogger<TransactionCreatedEventHandler> logger)
{
    public async Task HandleAsync(TransactionCreatedEvent @event, CancellationToken ct = default)
    {
        var date = @event.OccurredOn.Date;
        logger.LogInformation("Processing event {Id} for date {Date}", @event.TransactionId, date);

        var balance = await repository.GetByDateAsync(date, ct)
                      ?? DailyBalance.Create(date);

        var isNew = balance.TransactionCount == 0 && balance.TotalCredits == 0 && balance.TotalDebits == 0;

        if (@event.Type == "Credit")
            balance.ApplyCredit(@event.Amount);
        else
            balance.ApplyDebit(@event.Amount);

        if (isNew)
            await repository.AddAsync(balance, ct);
        else
            await repository.UpdateAsync(balance, ct);

        logger.LogInformation("Updated DailyBalance for {Date}: Balance={Balance}", date, balance.Balance);
    }
}
