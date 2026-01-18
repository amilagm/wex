using Microsoft.Extensions.Logging;
using Wex.Domain.Abstractions;
using Wex.Domain.Exceptions;
using Wex.Application.Models;
using Wex.Domain.Entities;
using Wex.Domain.ValueObjects;

namespace Wex.Application.Services;

public class PurchaseService(
    ICardRepository cardRepository,
    IPurchaseRepository purchaseRepository,
    ICurrencyConverter currencyConverter,
    ILogger<PurchaseService> logger) : IPurchaseService
{
    public async Task<PurchaseCreated> AddAsync(
        string cardNumber,
        string description,
        DateOnly transactionDate,
        decimal amountUsd,
        CancellationToken cancellationToken)
    {
        var card = await cardRepository.GetByNumberAsync(cardNumber, cancellationToken);
        if (card is null)
        {
            logger.LogWarning("Card {CardNumber} not found when adding purchase", cardNumber);
            throw new NotFoundException($"Card {cardNumber} was not found.");
        }

        var roundedAmount = Money.Round(amountUsd);

        var existingPurchases = await purchaseRepository.ListByCardIdAsync(card.Id, cancellationToken);
        var totalSpent = existingPurchases.Sum(p => p.Amount.Amount);

        if (totalSpent + roundedAmount > card.CreditLimit.Amount)
        {
            logger.LogWarning("Purchase of {Amount} would exceed credit limit for card {CardNumber}", roundedAmount, cardNumber);
            throw new ConflictException("Purchase would exceed card credit limit.");
        }

        var purchase = new PurchaseTransaction(
            Guid.NewGuid(),
            card.Id,
            description,
            transactionDate,
            Money.Usd(roundedAmount));

        await purchaseRepository.AddAsync(purchase, cancellationToken);

        logger.LogInformation("Created purchase {PurchaseId} for card {CardNumber} amount {Amount}",
            purchase.Id, cardNumber, amountUsd);

        return new PurchaseCreated(
            purchase.Id,
            purchase.CardId,
            purchase.Description,
            purchase.TransactionDate,
            purchase.Amount.Amount);
    }

    public async Task<ConvertedPurchase> GetConvertedAsync(
        Guid purchaseId,
        string currency,
        CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetByIdAsync(purchaseId, cancellationToken);
        if (purchase is null)
        {
            logger.LogWarning("Purchase {PurchaseId} not found", purchaseId);
            throw new NotFoundException($"Purchase {purchaseId} was not found.");
        }

        logger.LogDebug("Converting purchase {PurchaseId} to {Currency}", purchaseId, currency);
        var conversion = await currencyConverter.ConvertAsync(
            purchase.Amount.Amount,
            currency,
            purchase.TransactionDate,
            cancellationToken);

        return new ConvertedPurchase(
            Id: purchase.Id,
            Description: purchase.Description,
            TransactionDate: purchase.TransactionDate,
            AmountUsd: purchase.Amount.Amount,
            ConvertedCurrency: conversion.TargetCurrency,
            UsdToTargetRate: conversion.ExchangeRate,
            ExchangeRateDate: conversion.RateDate,
            ConvertedAmount: conversion.ConvertedAmount);
    }
}
