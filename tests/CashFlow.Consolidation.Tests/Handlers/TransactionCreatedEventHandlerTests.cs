using CashFlow.Consolidation.Application.Events;
using CashFlow.Consolidation.Domain.Entities;
using CashFlow.Consolidation.Domain.Interfaces;
using CashFlow.Shared.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CashFlow.Consolidation.Tests.Handlers;

public class TransactionCreatedEventHandlerTests
{
    private readonly IDailyBalanceRepository _repository = Substitute.For<IDailyBalanceRepository>();
    private readonly TransactionCreatedEventHandler _handler;

    public TransactionCreatedEventHandlerTests()
    {
        _handler = new TransactionCreatedEventHandler(_repository, NullLogger<TransactionCreatedEventHandler>.Instance);
    }

    [Fact]
    public async Task Handle_CreditEvent_FirstOfDay_ShouldCreateNewBalance()
    {
        var date   = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var @event = new TransactionCreatedEvent(Guid.NewGuid(), 300m, "Credit", "Sale", date);

        _repository.GetByDateAsync(date.Date, Arg.Any<CancellationToken>()).Returns((DailyBalance?)null);

        await _handler.HandleAsync(@event);

        await _repository.Received(1).AddAsync(
            Arg.Is<DailyBalance>(b => b.TotalCredits == 300m && b.TotalDebits == 0m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DebitEvent_ExistingBalance_ShouldUpdateDebit()
    {
        var date    = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var existing = DailyBalance.Create(date);
        existing.ApplyCredit(500m);

        _repository.GetByDateAsync(date.Date, Arg.Any<CancellationToken>()).Returns(existing);

        var @event = new TransactionCreatedEvent(Guid.NewGuid(), 200m, "Debit", "Expense", date);
        await _handler.HandleAsync(@event);

        await _repository.Received(1).UpdateAsync(
            Arg.Is<DailyBalance>(b => b.TotalDebits == 200m && b.TotalCredits == 500m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleEvents_ShouldAccumulateBalance()
    {
        var date    = DateTime.UtcNow.Date;
        var balance = DailyBalance.Create(date);

        balance.ApplyCredit(1000m);
        balance.ApplyDebit(300m);

        balance.Balance.Should().Be(700m);
        balance.TransactionCount.Should().Be(2);
    }
}
