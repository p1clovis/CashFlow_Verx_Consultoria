using CashFlow.Transactions.Application.Commands;
using CashFlow.Transactions.Application.Interfaces;
using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CashFlow.Transactions.Tests.Handlers;

public class CreateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repository = Substitute.For<ITransactionRepository>();
    private readonly IEventPublisher _publisher         = Substitute.For<IEventPublisher>();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(_repository, _publisher);
    }

    [Fact]
    public async Task Handle_ValidCreditCommand_ShouldPersistAndPublish()
    {
        var command = new CreateTransactionCommand(500m, "Credit", "Client payment", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Amount.Should().Be(500m);
        result.Type.Should().Be("Credit");

        await _repository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidDebitCommand_ShouldSucceed()
    {
        var command = new CreateTransactionCommand(200m, "Debit", "Supplier payment", null);
        var result  = await _handler.Handle(command, CancellationToken.None);
        result.Type.Should().Be("Debit");
    }

    [Fact]
    public async Task Handle_InvalidType_ShouldThrowArgumentException()
    {
        var command = new CreateTransactionCommand(100m, "INVALID", "Test", null);
        var act     = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid transaction type*");
    }

    [Fact]
    public async Task Handle_PublisherFails_ShouldStillReturnResult()
    {
        // Transaction must succeed even if event publishing fails (resilience requirement)
        _publisher
            .When(p => p.PublishAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("Broker unavailable"));

        var command = new CreateTransactionCommand(100m, "Credit", "Test", null);

        // Should NOT throw — publisher errors are swallowed in infrastructure
        // (this test validates that requirement at application level would not throw either,
        //  the actual swallow happens in RabbitMqEventPublisher)
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
    }
}
