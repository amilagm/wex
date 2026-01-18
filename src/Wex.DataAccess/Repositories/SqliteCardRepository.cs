using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Wex.DataAccess.Database;
using Wex.DataAccess.Exceptions;
using Wex.Domain.Abstractions;
using Wex.Domain.Entities;
using Wex.Domain.ValueObjects;

namespace Wex.DataAccess.Repositories;

public class SqliteCardRepository(
    IDbConnectionFactory connectionFactory,
    ILogger<SqliteCardRepository> logger) : ICardRepository
{
    public async Task<Card?> GetByNumberAsync(string number, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, CardNumber, CreditLimit
            FROM Cards
            WHERE CardNumber = @CardNumber
            """;

        try
        {
            using var connection = connectionFactory.Create();
            var row = await connection.QuerySingleOrDefaultAsync<CardRow>(
                new CommandDefinition(sql, new { CardNumber = number }, cancellationToken: cancellationToken));

            return row is null ? null : MapToCard(row);
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database error retrieving card by number");
            throw new InfrastructureException("Unable to retrieve card. Please try again later.", ex);
        }
    }

    public async Task AddAsync(Card card, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO Cards (Id, CardNumber, CreditLimit)
            VALUES (@Id, @CardNumber, @CreditLimit)
            """;

        try
        {
            using var connection = connectionFactory.Create();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = card.Id.ToString(),
                        CardNumber = card.Number,
                        CreditLimit = card.CreditLimit.Amount
                    },
                    cancellationToken: cancellationToken));
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database error adding card {CardId}", card.Id);
            throw new InfrastructureException("Unable to save card. Please try again later.", ex);
        }
    }

    private static Card MapToCard(CardRow row) =>
        new(Guid.Parse(row.Id), row.CardNumber, Money.Usd(row.CreditLimit));

    private sealed class CardRow
    {
        public string Id { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public decimal CreditLimit { get; set; }
    }
}
