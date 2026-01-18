using System;
using System.Threading;
using System.Threading.Tasks;
using Wex.Domain.Abstractions;

namespace Wex.Tests.Support;

public sealed class FakeExchangeRateProvider : IExchangeRateProvider
{
    private readonly Func<string, DateOnly, ExchangeRateResult> _handler;

    public FakeExchangeRateProvider(Func<string, DateOnly, ExchangeRateResult> handler)
    {
        _handler = handler;
    }

    public Task<ExchangeRateResult> GetRateAsync(string currency, DateOnly purchaseDate, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handler(currency, purchaseDate));
    }
}
