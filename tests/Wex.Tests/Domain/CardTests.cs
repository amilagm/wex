using Wex.Domain.Entities;
using Wex.Domain.Exceptions;
using Wex.Domain.ValueObjects;
using Xunit;

namespace Wex.Tests.Domain;

public sealed class CardTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesCard()
    {
        var id = Guid.NewGuid();
        var number = "1234567890123456";
        var limit = Money.Usd(500m);

        var card = new Card(id, number, limit);

        Assert.Equal(id, card.Id);
        Assert.Equal(number, card.Number);
        Assert.Equal(limit, card.CreditLimit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456789012345a")]
    public void Constructor_InvalidCardNumber_ThrowsDomainException(string number)
    {
        Assert.Throws<DomainException>(() => new Card(Guid.NewGuid(), number, Money.Usd(500m)));
    }
}
