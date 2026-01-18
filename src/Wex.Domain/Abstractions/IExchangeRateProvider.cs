namespace Wex.Domain.Abstractions;

public interface IExchangeRateProvider
{
    Task<ExchangeRateResult> GetRateAsync(string currency, DateOnly purchaseDate, CancellationToken cancellationToken);
}

public sealed record ExchangeRateResult(DateOnly RateDate, decimal Rate, string Currency);
