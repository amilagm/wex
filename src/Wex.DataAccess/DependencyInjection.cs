using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wex.DataAccess.Database;
using Wex.DataAccess.Repositories;
using Wex.Domain.Abstractions;

namespace Wex.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString configuration is required.");

        services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<DatabaseInitializer>();

        services.AddScoped<ICardRepository, SqliteCardRepository>();
        services.AddScoped<IPurchaseRepository, SqlitePurchaseRepository>();

        return services;
    }
}
