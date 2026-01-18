using Wex.Domain;

namespace Wex.Application.Services;

public sealed record ConversionResult(
    decimal OriginalAmountUsd,
    string TargetCurrency,
    decimal ExchangeRate,
    DateOnly RateDate,
    decimal ConvertedAmount)
{
    public bool IsBaseCurrency => string.Equals(TargetCurrency, Constants.BaseCurrency, StringComparison.OrdinalIgnoreCase);
}
