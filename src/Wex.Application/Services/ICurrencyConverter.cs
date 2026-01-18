namespace Wex.Application.Services;

public interface ICurrencyConverter
{
    Task<ConversionResult> ConvertAsync(
        decimal amountUsd,
        string targetCurrency,
        DateOnly asOfDate,
        CancellationToken cancellationToken);
}
