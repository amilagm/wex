using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Wex.Application.Services;
using Wex.Domain;
using Wex.Domain.Exceptions;
using Xunit;

namespace Wex.Tests.Application;

public sealed class TreasuryExchangeRateProviderTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TreasuryExchangeRateProviderTests()
    {
        _httpClient = new HttpClient(_handler) { BaseAddress = new Uri("https://api.test.gov/") };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TreasuryApiUrl"] = "https://api.test.gov/rates"
            })
            .Build();
    }

    public void Dispose() => _httpClient.Dispose();

    private TreasuryExchangeRateProvider CreateProvider() =>
        new(_httpClient, _configuration, NullLogger<TreasuryExchangeRateProvider>.Instance);

    [Fact]
    public async Task GetRateAsync_RateExactly6MonthsOld_Succeeds()
    {
        // Requirement: "must use a currency conversion rate...from within the last 6 months"
        // Boundary: exactly 6 months should still be valid
        var purchaseDate = new DateOnly(2024, 06, 15);
        var rateDate = purchaseDate.AddMonths(-Constants.ExchangeRateValidityMonths);
        _handler.SetResponse(CreateApiResponse(rateDate, 1.10m));

        var provider = CreateProvider();
        var result = await provider.GetRateAsync("CAD", purchaseDate, default);

        Assert.Equal(rateDate, result.RateDate);
        Assert.Equal(1.10m, result.Rate);
        Assert.Equal("CAD", result.Currency);
    }

    [Fact]
    public async Task GetRateAsync_RateOlderThan6Months_ThrowsConversionException()
    {
        // Requirement: "If no currency conversion rate is available within 6 months...an error should be returned"
        var purchaseDate = new DateOnly(2024, 06, 15);
        var rateDate = purchaseDate.AddMonths(-Constants.ExchangeRateValidityMonths).AddDays(-1);
        _handler.SetResponse(CreateApiResponse(rateDate, 1.25m));

        var provider = CreateProvider();

        var ex = await Assert.ThrowsAsync<ConversionException>(() =>
            provider.GetRateAsync("EUR", purchaseDate, default));

        Assert.Contains("6 months", ex.Message);
    }

    [Fact]
    public async Task GetRateAsync_RateDateAfterPurchaseDate_ThrowsConversionException()
    {
        // Requirement: "must use a currency conversion rate less than or equal to the purchase date"
        var purchaseDate = new DateOnly(2024, 06, 15);
        var rateDate = new DateOnly(2024, 06, 16);
        _handler.SetResponse(CreateApiResponse(rateDate, 1.25m));

        var provider = CreateProvider();

        await Assert.ThrowsAsync<ConversionException>(() =>
            provider.GetRateAsync("EUR", purchaseDate, default));
    }

    [Fact]
    public async Task GetRateAsync_UnsupportedCurrency_ThrowsConversionException()
    {
        var provider = CreateProvider();

        var ex = await Assert.ThrowsAsync<ConversionException>(() =>
            provider.GetRateAsync("XYZ", new DateOnly(2024, 06, 15), default));

        Assert.Contains("not supported", ex.Message);
    }

    [Fact]
    public async Task GetRateAsync_NoDataFromApi_ThrowsConversionException()
    {
        // API returns empty data for a supported currency
        _handler.SetResponse("""{"data":[]}""");

        var provider = CreateProvider();

        var ex = await Assert.ThrowsAsync<ConversionException>(() =>
            provider.GetRateAsync("EUR", new DateOnly(2024, 06, 15), default));

        Assert.Contains("No exchange rate available", ex.Message);
    }

    private static string CreateApiResponse(DateOnly rateDate, decimal rate) =>
        $$"""{"data":[{"record_date":"{{rateDate:yyyy-MM-dd}}","exchange_rate":"{{rate}}"}]}""";

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _response = new(HttpStatusCode.OK);

        public void SetResponse(string json)
        {
            _response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
