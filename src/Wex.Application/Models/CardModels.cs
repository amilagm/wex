namespace Wex.Application.Models;

public sealed record CardCreated(Guid Id, string CardNumber, decimal CreditLimitUsd);

public sealed record CardBalance(
    Guid CardId,
    decimal CreditLimitUsd,
    decimal TotalPurchasesUsd,
    decimal AvailableUsd,
    string ConvertedCurrency,
    decimal? UsdToTargetRate,
    DateOnly? ExchangeRateDate,
    decimal? AvailableInTargetCurrency);
