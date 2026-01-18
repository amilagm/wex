using Microsoft.Extensions.Logging.Abstractions;
using Wex.Application.Services;
using Wex.Domain.Exceptions;
using Wex.Domain.ValueObjects;
using Wex.Tests.Support;
using Xunit;

namespace Wex.Tests.Application;

public sealed class PurchaseServiceTests
{
    private readonly InMemoryCardRepository _cardRepo = new();
    private readonly InMemoryPurchaseRepository _purchaseRepo = new();
    private readonly FakeExchangeRateProvider _rateProvider = new((currency, _) => new(new DateOnly(2024, 01, 01), 1.5m, currency.ToUpperInvariant()));

    private PurchaseService CreateService()
    {
        var currencyConverter = new CurrencyConverter(_rateProvider);
        return new(_cardRepo, _purchaseRepo, currencyConverter, NullLogger<PurchaseService>.Instance);
    }

    [Fact]
    public async Task AddAsync_ValidInput_ReturnsPurchaseCreated()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();
        var result = await service.AddAsync(card.Number, "Coffee", new DateOnly(2024, 01, 15), 5.50m, default);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(card.Id, result.CardId);
        Assert.Equal("Coffee", result.Description);
        Assert.Equal(5.50m, result.AmountUsd);
    }

    [Fact]
    public async Task AddAsync_CardNotFound_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddAsync("9999999999999999", "Coffee", new DateOnly(2024, 01, 15), 5.50m, default));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddAsync_EmptyDescription_ThrowsDomainException(string? description)
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AddAsync(card.Number, description!, new DateOnly(2024, 01, 15), 5.50m, default));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task AddAsync_InvalidAmount_ThrowsDomainException(decimal invalidAmount)
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AddAsync(card.Number, "Coffee", new DateOnly(2024, 01, 15), invalidAmount, default));
    }

    [Fact]
    public async Task AddAsync_ExceedsCreditLimit_ThrowsConflictException()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AddAsync(card.Number, "Expensive item", new DateOnly(2024, 01, 15), 150m, default));
    }

    [Fact]
    public async Task AddAsync_ExactlyAtCreditLimit_Succeeds()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);

        var service = CreateService();
        var result = await service.AddAsync(card.Number, "Max purchase", new DateOnly(2024, 01, 15), 100m, default);

        Assert.Equal(100m, result.AmountUsd);
    }

    [Fact]
    public async Task AddAsync_WithExistingPurchases_ValidatesCumulativeLimit()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(100m));
        await _cardRepo.AddAsync(card, default);
        await _purchaseRepo.AddAsync(new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "First", new DateOnly(2024, 01, 01), Money.Usd(60m)), default);

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AddAsync(card.Number, "Second", new DateOnly(2024, 01, 15), 50m, default));
    }

    [Fact]
    public async Task GetConvertedAsync_PurchaseNotFound_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetConvertedAsync(Guid.NewGuid(), "USD", default));
    }

    [Fact]
    public async Task GetConvertedAsync_UsdCurrency_ReturnsWithoutConversion()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var purchase = new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "Hotel", new DateOnly(2024, 02, 01), Money.Usd(100m));
        await _purchaseRepo.AddAsync(purchase, default);

        var service = CreateService();
        var result = await service.GetConvertedAsync(purchase.Id, "USD", default);

        Assert.Equal(100m, result.AmountUsd);
        Assert.Equal(100m, result.ConvertedAmount);
        Assert.Equal(1m, result.UsdToTargetRate);
    }

    [Fact]
    public async Task GetConvertedAsync_ForeignCurrency_ReturnsConvertedAmount()
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var purchase = new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "Hotel", new DateOnly(2024, 02, 01), Money.Usd(100m));
        await _purchaseRepo.AddAsync(purchase, default);

        var service = CreateService();
        var result = await service.GetConvertedAsync(purchase.Id, "EUR", default);

        Assert.Equal(150m, result.ConvertedAmount); // 100 * 1.5
        Assert.Equal("EUR", result.ConvertedCurrency);
        Assert.Equal(1.5m, result.UsdToTargetRate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetConvertedAsync_EmptyCurrency_ThrowsDomainException(string? currency)
    {
        var card = new Wex.Domain.Entities.Card(Guid.NewGuid(), "1234567890123456", Money.Usd(500m));
        await _cardRepo.AddAsync(card, default);

        var purchase = new Wex.Domain.Entities.PurchaseTransaction(Guid.NewGuid(), card.Id, "Hotel", new DateOnly(2024, 02, 01), Money.Usd(100m));
        await _purchaseRepo.AddAsync(purchase, default);

        var service = CreateService();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.GetConvertedAsync(purchase.Id, currency!, default));
    }
}
