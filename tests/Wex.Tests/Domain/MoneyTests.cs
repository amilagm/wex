using Wex.Domain.Exceptions;
using Wex.Domain.ValueObjects;
using Xunit;

namespace Wex.Tests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void From_RoundsToTwoDecimals()
    {
        var money = Money.Usd(10.555m);
        Assert.Equal(10.56m, money.Amount);
    }

    [Fact]
    public void From_NonPositive_Throws()
    {
        Assert.Throws<DomainException>(() => Money.Usd(0m));
    }
}
