using System;
using System.Threading.Tasks;
using Wex.Application.Services;
using Wex.Domain;
using Wex.Domain.Exceptions;
using Wex.Tests.Support;
using Xunit;

namespace Wex.Tests.Application;

public sealed class CurrencyConverterTests
{
    private readonly FakeExchangeRateProvider _rateProvider = new((currency, _) =>
        new(new DateOnly(2024, 01, 15), 1.25m, currency.ToUpperInvariant()));

    private CurrencyConverter CreateConverter() => new(_rateProvider);

    [Fact]
    public async Task ConvertAsync_BaseCurrency_ReturnsRateOfOne()
    {
        var converter = CreateConverter();

        var result = await converter.ConvertAsync(100m, Constants.BaseCurrency, new DateOnly(2024, 01, 15), default);

        Assert.Equal(100m, result.OriginalAmountUsd);
        Assert.Equal(Constants.BaseCurrency, result.TargetCurrency);
        Assert.Equal(1m, result.ExchangeRate);
        Assert.Equal(100m, result.ConvertedAmount);
        Assert.True(result.IsBaseCurrency);
    }

    [Fact]
    public async Task ConvertAsync_ForeignCurrency_AppliesExchangeRate()
    {
        var converter = CreateConverter();

        var result = await converter.ConvertAsync(100m, "EUR", new DateOnly(2024, 01, 15), default);

        Assert.Equal(100m, result.OriginalAmountUsd);
        Assert.Equal("EUR", result.TargetCurrency);
        Assert.Equal(1.25m, result.ExchangeRate);
        Assert.Equal(125m, result.ConvertedAmount);
        Assert.False(result.IsBaseCurrency);
    }

    [Fact]
    public async Task ConvertAsync_RoundsToTwoDecimalPlaces()
    {
        var rateProvider = new FakeExchangeRateProvider((currency, _) =>
            new(new DateOnly(2024, 01, 15), 1.333m, currency.ToUpperInvariant()));
        var converter = new CurrencyConverter(rateProvider);

        var result = await converter.ConvertAsync(100m, "GBP", new DateOnly(2024, 01, 15), default);

        Assert.Equal(133.30m, result.ConvertedAmount); // 100 * 1.333 = 133.3, rounded to 133.30
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ConvertAsync_EmptyCurrency_ThrowsDomainException(string? currency)
    {
        var converter = CreateConverter();

        await Assert.ThrowsAsync<DomainException>(() =>
            converter.ConvertAsync(100m, currency!, new DateOnly(2024, 01, 15), default));
    }

    [Fact]
    public async Task ConvertAsync_NormalizesToUpperCase()
    {
        var converter = CreateConverter();

        var result = await converter.ConvertAsync(100m, "eur", new DateOnly(2024, 01, 15), default);

        Assert.Equal("EUR", result.TargetCurrency);
    }

    [Fact]
    public async Task ConvertAsync_BaseCurrencyLowerCase_ReturnsRateOfOne()
    {
        var converter = CreateConverter();

        var result = await converter.ConvertAsync(100m, "usd", new DateOnly(2024, 01, 15), default);

        Assert.True(result.IsBaseCurrency);
        Assert.Equal(1m, result.ExchangeRate);
    }

    [Fact]
    public async Task ConvertAsync_ZeroAmount_ReturnsZeroWithoutCallingProvider()
    {
        var providerCalled = false;
        var rateProvider = new FakeExchangeRateProvider((currency, _) =>
        {
            providerCalled = true;
            return new(new DateOnly(2024, 01, 15), 1.25m, currency.ToUpperInvariant());
        });
        var converter = new CurrencyConverter(rateProvider);

        var result = await converter.ConvertAsync(0m, "EUR", new DateOnly(2024, 01, 15), default);

        Assert.Equal(0m, result.OriginalAmountUsd);
        Assert.Equal(0m, result.ConvertedAmount);
        Assert.Equal("EUR", result.TargetCurrency);
        Assert.False(providerCalled);
    }
}
