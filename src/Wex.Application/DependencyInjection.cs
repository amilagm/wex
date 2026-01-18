using Microsoft.Extensions.DependencyInjection;
using Wex.Application.Services;
using Wex.Domain.Abstractions;

namespace Wex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpClient<IExchangeRateProvider, TreasuryExchangeRateProvider>();
        services.AddScoped<ICurrencyConverter, CurrencyConverter>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        return services;
    }
}
