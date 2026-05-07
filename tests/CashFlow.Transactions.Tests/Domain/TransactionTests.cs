using CashFlow.Transactions.Domain.Entities;
using CashFlow.Transactions.Domain.Enums;
using CashFlow.Transactions.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CashFlow.Transactions.Tests.Domain;

public class TransactionTests
{
    [Fact]
    public void Create_ValidCredit_ShouldSucceed()
    {
        var tx = Transaction.Create(100m, TransactionType.Credit, "Salary");
        tx.Amount.Should().Be(100m);
        tx.Type.Should().Be(TransactionType.Credit);
        tx.IsCredit.Should().BeTrue();
        tx.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ValidDebit_ShouldSucceed()
    {
        var tx = Transaction.Create(50m, TransactionType.Debit, "Rent");
        tx.IsDebit.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ZeroOrNegativeAmount_ShouldThrow(decimal amount)
    {
        var act = () => Transaction.Create(amount, TransactionType.Credit, "Test");
        act.Should().Throw<TransactionDomainException>()
           .WithMessage("*greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyDescription_ShouldThrow(string? description)
    {
        var act = () => Transaction.Create(100m, TransactionType.Credit, description!);
        act.Should().Throw<TransactionDomainException>()
           .WithMessage("*Description*");
    }

    [Fact]
    public void Create_WithExplicitDate_ShouldUseProvidedDate()
    {
        var date = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var tx   = Transaction.Create(200m, TransactionType.Debit, "Payment", date);
        tx.OccurredOn.Should().Be(date);
    }

    [Fact]
    public void Create_WithoutDate_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var tx     = Transaction.Create(100m, TransactionType.Credit, "Test");
        tx.OccurredOn.Should().BeAfter(before);
    }
}
