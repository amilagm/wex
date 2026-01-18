using Microsoft.Extensions.Logging.Abstractions;
using Wex.Domain.Exceptions;
using Wex.Application.Services;
using Wex.Domain.ValueObjects;
using Wex.Tests.Support;
using Xunit;

namespace Wex.Tests.Application;

public sealed class CardServiceTests
{
    private readonly InMemoryCardRepository _cardRepo = new();
    private readonly InMemoryPurchaseRepository _purchaseRepo = new();
    private readonly FakeExchangeRateProvider _rateProvider = new((currency, _) => new(new DateOnly(2024, 01, 01), 2m, currency.ToUpperInvariant()));

    private CardService CreateService()
    {
        var currencyConverter = new CurrencyConverter(_rateProvider);
        return new(_cardRepo, _purchaseRepo, currencyConverter, NullLogger<CardService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCardCreated()
    {
        var service = CreateService();

        var result = await service.CreateAsync("1234567890123456", 500m, default);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("1234567890123456", result.CardNumber);
        Assert.Equal(500m, result.CreditLimitUsd);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCardNumber_ThrowsConflictException()
    {
        var existing = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(existing, default);

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync("1234567890123456", 200m, default));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CreateAsync_InvalidCreditLimit_ThrowsDomainException(decimal invalidLimit)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync("1234567890123456", invalidLimit, default));
    }

    [Fact]
    public async Task CreateAsync_InvalidCardNumber_ThrowsDomainException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync("invalid", 100m, default));
    }

    [Fact]
    public async Task GetBalanceAsync_CardNotFound_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetBalanceAsync("9999999999999999", "USD", DateOnly.FromDateTime(DateTime.UtcNow), default));
    }

    [Fact]
    public async Task GetBalanceAsync_UsdCurrency_ReturnsWithoutConversion()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);
        await _purchaseRepo.AddAsync(new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "Fuel", new DateOnly(2024, 01, 01), Money.Usd(10m)), default);

        var service = CreateService();
        var balance = await service.GetBalanceAsync(card.Number, "USD", new DateOnly(2024, 01, 02), default);

        Assert.Equal(90m, balance.AvailableUsd);
        Assert.Equal(90m, balance.AvailableInTargetCurrency);
        Assert.Null(balance.UsdToTargetRate);
    }

    [Fact]
    public async Task GetBalanceAsync_ForeignCurrency_ConvertsUsingRate()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);
        await _purchaseRepo.AddAsync(new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "Fuel", new DateOnly(2024, 01, 01), Money.Usd(10m)), default);

        var service = CreateService();
        var balance = await service.GetBalanceAsync(card.Number, "EUR", new DateOnly(2024, 01, 02), default);

        Assert.Equal(90m, balance.AvailableUsd);
        Assert.Equal(180m, balance.AvailableInTargetCurrency); // 90 * 2
        Assert.Equal(2m, balance.UsdToTargetRate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetBalanceAsync_EmptyCurrency_ThrowsDomainException(string? currency)
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.GetBalanceAsync(card.Number, currency!, DateOnly.FromDateTime(DateTime.UtcNow), default));
    }
}
