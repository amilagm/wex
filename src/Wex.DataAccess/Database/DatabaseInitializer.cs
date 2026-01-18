using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Wex.DataAccess.Exceptions;

namespace Wex.DataAccess.Database;

public class DatabaseInitializer(
    IDbConnectionFactory connectionFactory,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS Cards (
                Id TEXT PRIMARY KEY,
                CardNumber TEXT NOT NULL UNIQUE,
                CreditLimit REAL NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Purchases (
                Id TEXT PRIMARY KEY,
                CardId TEXT NOT NULL,
                Description TEXT NOT NULL,
                TransactionDate TEXT NOT NULL,
                Amount REAL NOT NULL,
                FOREIGN KEY(CardId) REFERENCES Cards(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_Purchases_CardId ON Purchases(CardId);
            """;

        try
        {
            using var connection = connectionFactory.Create();
            await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database initialization failed");
            throw new InfrastructureException("Unable to initialize database. Please try again later.", ex);
        }
    }
}
