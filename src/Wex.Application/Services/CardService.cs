using Microsoft.Extensions.Logging;
using Wex.Domain.Abstractions;
using Wex.Domain.Exceptions;
using Wex.Application.Models;
using Wex.Domain.Entities;
using Wex.Domain.ValueObjects;

namespace Wex.Application.Services;

public class CardService(
    ICardRepository cardRepository,
    IPurchaseRepository purchaseRepository,
    ICurrencyConverter currencyConverter,
    ILogger<CardService> logger) : ICardService
{
    public async Task<CardCreated> CreateAsync(
        string cardNumber,
        decimal creditLimitUsd,
        CancellationToken cancellationToken)
    {
        var existing = await cardRepository.GetByNumberAsync(cardNumber, cancellationToken);

        if (existing is not null)
        {
            logger.LogWarning("Attempted to create duplicate card with number {CardNumber}", cardNumber);
            throw new ConflictException($"Card with number {cardNumber} already exists.");
        }

        var card = new Card(Guid.NewGuid(), cardNumber, Money.Usd(creditLimitUsd));

        await cardRepository.AddAsync(card, cancellationToken);

        logger.LogInformation("Created card {CardId} with credit limit {CreditLimit}", card.Id, creditLimitUsd);

        return new CardCreated(card.Id, card.Number, card.CreditLimit.Amount);
    }

    public async Task<CardBalance> GetBalanceAsync(
        string cardNumber,
        string currency,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        var card = await cardRepository.GetByNumberAsync(cardNumber, cancellationToken);

        if (card is null)
        {
            logger.LogWarning("Card {CardNumber} not found", cardNumber);
            throw new NotFoundException($"Card {cardNumber} was not found.");
        }

        var purchases = await purchaseRepository.ListByCardIdAsync(card.Id, cancellationToken);

        var totalPurchases = purchases.Sum(p => p.Amount.Amount);

        var availableUsd = card.CreditLimit.Amount - totalPurchases;

        logger.LogDebug("Converting balance for card {CardNumber} to {Currency}", cardNumber, currency);

        var conversion = await currencyConverter.ConvertAsync(availableUsd, currency, asOfDate, cancellationToken);

        return new CardBalance(
            CardId: card.Id,
            CreditLimitUsd: card.CreditLimit.Amount,
            TotalPurchasesUsd: totalPurchases,
            AvailableUsd: availableUsd,
            ConvertedCurrency: conversion.TargetCurrency,
            UsdToTargetRate: conversion.IsBaseCurrency ? null : conversion.ExchangeRate,
            ExchangeRateDate: conversion.IsBaseCurrency ? null : conversion.RateDate,
            AvailableInTargetCurrency: conversion.ConvertedAmount);
    }
}
