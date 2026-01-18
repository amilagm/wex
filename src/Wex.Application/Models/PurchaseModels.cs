namespace Wex.Application.Models;

public sealed record PurchaseCreated(
    Guid Id,
    Guid CardId,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd);

public sealed record ConvertedPurchase(
    Guid Id,
    string Description,
    DateOnly TransactionDate,
    decimal AmountUsd,
    string ConvertedCurrency,
    decimal UsdToTargetRate,
    DateOnly ExchangeRateDate,
    decimal ConvertedAmount);
