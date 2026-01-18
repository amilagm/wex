using Wex.Domain.Entities;
using Wex.Domain.Exceptions;
using Wex.Domain.ValueObjects;
using Xunit;

namespace Wex.Tests.Domain;

public sealed class PurchaseTransactionTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesPurchase()
    {
        var id = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var description = "Coffee";
        var date = new DateOnly(2024, 01, 15);
        var amount = Money.Usd(5.50m);

        var purchase = new PurchaseTransaction(id, cardId, description, date, amount);

        Assert.Equal(id, purchase.Id);
        Assert.Equal(cardId, purchase.CardId);
        Assert.Equal("Coffee", purchase.Description);
        Assert.Equal(date, purchase.TransactionDate);
        Assert.Equal(amount, purchase.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyDescription_ThrowsDomainException(string? description)
    {
        Assert.Throws<DomainException>(() =>
            new PurchaseTransaction(Guid.NewGuid(), Guid.NewGuid(), description!, new DateOnly(2024, 01, 15), Money.Usd(5m)));
    }

    [Fact]
    public void Constructor_DescriptionTooLong_ThrowsDomainException()
    {
        var longDescription = new string('a', 51);

        Assert.Throws<DomainException>(() =>
            new PurchaseTransaction(Guid.NewGuid(), Guid.NewGuid(), longDescription, new DateOnly(2024, 01, 15), Money.Usd(5m)));
    }

    [Fact]
    public void Constructor_DescriptionExactly50Chars_Succeeds()
    {
        var description = new string('a', 50);

        var purchase = new PurchaseTransaction(Guid.NewGuid(), Guid.NewGuid(), description, new DateOnly(2024, 01, 15), Money.Usd(5m));

        Assert.Equal(50, purchase.Description.Length);
    }

    [Fact]
    public void Constructor_TrimsDescription()
    {
        var purchase = new PurchaseTransaction(Guid.NewGuid(), Guid.NewGuid(), "  Coffee  ", new DateOnly(2024, 01, 15), Money.Usd(5m));

        Assert.Equal("Coffee", purchase.Description);
    }
}
