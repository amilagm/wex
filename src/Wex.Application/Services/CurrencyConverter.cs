using Wex.Domain;
using Wex.Domain.Abstractions;
using Wex.Domain.ValueObjects;

namespace Wex.Application.Services;

public class CurrencyConverter(IExchangeRateProvider exchangeRateProvider) : ICurrencyConverter
{
    public async Task<ConversionResult> ConvertAsync(
        decimal amountUsd,
        string targetCurrency,
        DateOnly asOfDate,
        CancellationToken cancellationToken)
    {
        Guard.NotNullOrWhiteSpace(targetCurrency, "Currency");

        var normalizedCurrency = targetCurrency.Trim().ToUpperInvariant();

        // Zero balance converts to zero in any currency
        if (amountUsd == 0)
        {
            return new ConversionResult(
                OriginalAmountUsd: 0m,
                TargetCurrency: normalizedCurrency,
                ExchangeRate: 1m,
                RateDate: asOfDate,
                ConvertedAmount: 0m);
        }

        if (string.Equals(normalizedCurrency, Constants.BaseCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new ConversionResult(
                OriginalAmountUsd: amountUsd,
                TargetCurrency: Constants.BaseCurrency,
                ExchangeRate: 1m,
                RateDate: asOfDate,
                ConvertedAmount: Money.Round(amountUsd));
        }

        var rate = await exchangeRateProvider.GetRateAsync(normalizedCurrency, asOfDate, cancellationToken);

        var converted = Money.Round(amountUsd * rate.Rate);

        return new ConversionResult(
            OriginalAmountUsd: amountUsd,
            TargetCurrency: rate.Currency,
            ExchangeRate: rate.Rate,
            RateDate: rate.RateDate,
            ConvertedAmount: converted);
    }
}
