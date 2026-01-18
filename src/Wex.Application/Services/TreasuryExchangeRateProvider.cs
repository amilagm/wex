using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Wex.Domain;
using Wex.Domain.Abstractions;
using Wex.Domain.Exceptions;

namespace Wex.Application.Services;

public class TreasuryExchangeRateProvider(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TreasuryExchangeRateProvider> logger) : IExchangeRateProvider
{
    private readonly string _baseUrl = configuration["TreasuryApiUrl"]
        ?? throw new InvalidOperationException("TreasuryApiUrl configuration is required.");

    private static readonly Dictionary<string, string> CurrencyCodeToCountry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EUR"] = "Euro Zone",
        ["GBP"] = "United Kingdom",
        ["CAD"] = "Canada",
        ["JPY"] = "Japan",
        ["AUD"] = "Australia",
        ["CHF"] = "Switzerland",
        ["MXN"] = "Mexico",
        ["CNY"] = "China",
        ["INR"] = "India",
        ["BRL"] = "Brazil",
        ["KRW"] = "Korea",
        ["SGD"] = "Singapore",
        ["HKD"] = "Hong Kong",
        ["SEK"] = "Sweden",
        ["NOK"] = "Norway",
        ["DKK"] = "Denmark",
        ["NZD"] = "New Zealand",
        ["ZAR"] = "South Africa",
        ["THB"] = "Thailand",
        ["PHP"] = "Philippines",
    };

    public async Task<ExchangeRateResult> GetRateAsync(
        string currency,
        DateOnly purchaseDate,
        CancellationToken cancellationToken)
    {
        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        var requestUri = BuildExchangeRateRequestUri(normalizedCurrency, purchaseDate);

        TreasuryApiResponse? apiResponse;
        try
        {
            apiResponse = await httpClient.GetFromJsonAsync<TreasuryApiResponse>(requestUri, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Treasury API request failed for currency {Currency}", normalizedCurrency);
            throw new ConversionException("Unable to retrieve exchange rate. Please try again later.", ex);
        }

        if (apiResponse?.Data is not { Count: > 0 })
        {
            throw new ConversionException(
                $"No exchange rate available for {normalizedCurrency} on or before {purchaseDate:yyyy-MM-dd}.");
        }

        var entry = apiResponse.Data[0];
        var rateDate = ParseDate(entry.RecordDate, logger);
        var rate = ParseRate(entry.ExchangeRate, logger);

        if (rateDate > purchaseDate || rateDate < purchaseDate.AddMonths(-Constants.ExchangeRateValidityMonths))
        {
            throw new ConversionException(
                $"No exchange rate within {Constants.ExchangeRateValidityMonths} months of {purchaseDate:yyyy-MM-dd} for {normalizedCurrency}.");
        }

        return new ExchangeRateResult(rateDate, rate, normalizedCurrency);
    }

    private Uri BuildExchangeRateRequestUri(string currencyCode, DateOnly purchaseDate)
    {
        if (!CurrencyCodeToCountry.TryGetValue(currencyCode, out var country))
        {
            throw new ConversionException($"Currency {currencyCode} is not supported.");
        }

        var filter = $"record_date:lte:{purchaseDate:yyyy-MM-dd},country:eq:{country}";
        var query = $"filter={Uri.EscapeDataString(filter)}&sort=-record_date&page[size]=1";
        return new Uri($"{_baseUrl}?{query}");
    }

    private static DateOnly ParseDate(string? value, ILogger logger)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        logger.LogError("Exchange rate provider returned invalid date format: {Value}", value);
        throw new ConversionException("Unable to process exchange rate data. Please try again later.");
    }

    private static decimal ParseRate(string? value, ILogger logger)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
        {
            return rate;
        }

        logger.LogError("Exchange rate provider returned invalid rate format: {Value}", value);
        throw new ConversionException("Unable to process exchange rate data. Please try again later.");
    }

    private sealed class TreasuryApiResponse
    {
        public List<ExchangeRateEntry>? Data { get; init; }
    }

    private sealed class ExchangeRateEntry
    {
        [JsonPropertyName("record_date")]
        public string? RecordDate { get; init; }

        [JsonPropertyName("exchange_rate")]
        public string? ExchangeRate { get; init; }
    }
}
