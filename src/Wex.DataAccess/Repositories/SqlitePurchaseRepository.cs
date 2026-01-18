using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Wex.DataAccess.Database;
using Wex.DataAccess.Exceptions;
using Wex.Domain.Abstractions;
using Wex.Domain.Entities;
using Wex.Domain.ValueObjects;

namespace Wex.DataAccess.Repositories;

public class SqlitePurchaseRepository(
    IDbConnectionFactory connectionFactory,
    ILogger<SqlitePurchaseRepository> logger) : IPurchaseRepository
{
    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, CardId, Description, TransactionDate, Amount
            FROM Purchases
            WHERE Id = @Id
            """;

        try
        {
            using var connection = connectionFactory.Create();
            var row = await connection.QuerySingleOrDefaultAsync<PurchaseRow>(
                new CommandDefinition(sql, new { Id = id.ToString() }, cancellationToken: cancellationToken));

            return row is null ? null : MapToPurchase(row);
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database error retrieving purchase {PurchaseId}", id);
            throw new InfrastructureException("Unable to retrieve purchase. Please try again later.", ex);
        }
    }

    public async Task<IReadOnlyList<PurchaseTransaction>> ListByCardIdAsync(
        Guid cardId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, CardId, Description, TransactionDate, Amount
            FROM Purchases
            WHERE CardId = @CardId
            ORDER BY TransactionDate
            """;

        try
        {
            using var connection = connectionFactory.Create();
            var rows = await connection.QueryAsync<PurchaseRow>(
                new CommandDefinition(sql, new { CardId = cardId.ToString() }, cancellationToken: cancellationToken));

            return rows.Select(MapToPurchase).ToArray();
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database error listing purchases for card {CardId}", cardId);
            throw new InfrastructureException("Unable to retrieve purchases. Please try again later.", ex);
        }
    }

    public async Task AddAsync(PurchaseTransaction purchase, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO Purchases (Id, CardId, Description, TransactionDate, Amount)
            VALUES (@Id, @CardId, @Description, @TransactionDate, @Amount)
            """;

        try
        {
            using var connection = connectionFactory.Create();
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = purchase.Id.ToString(),
                        CardId = purchase.CardId.ToString(),
                        purchase.Description,
                        TransactionDate = purchase.TransactionDate.ToString("yyyy-MM-dd"),
                        Amount = purchase.Amount.Amount
                    },
                    cancellationToken: cancellationToken));
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Database error adding purchase {PurchaseId}", purchase.Id);
            throw new InfrastructureException("Unable to save purchase. Please try again later.", ex);
        }
    }

    private static PurchaseTransaction MapToPurchase(PurchaseRow row) =>
        new(
            Guid.Parse(row.Id),
            Guid.Parse(row.CardId),
            row.Description,
            DateOnly.Parse(row.TransactionDate),
            Money.Usd(row.Amount));

    private sealed class PurchaseRow
    {
        public string Id { get; set; } = "";
        public string CardId { get; set; } = "";
        public string Description { get; set; } = "";
        public string TransactionDate { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
