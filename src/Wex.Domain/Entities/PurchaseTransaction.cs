using System;
using Wex.Domain.ValueObjects;

namespace Wex.Domain.Entities;

public class PurchaseTransaction
{
    public Guid Id { get; }
    public Guid CardId { get; }
    public string Description { get; }
    public DateOnly TransactionDate { get; }
    public Money Amount { get; }

    public PurchaseTransaction(Guid id, Guid cardId, string description, DateOnly transactionDate, Money amount)
    {
        Guard.NotEmpty(id, "Purchase ID");
        Guard.NotEmpty(cardId, "Card ID");
        var trimmedDescription = Guard.NotNullOrWhiteSpace(description, "Description").Trim();
        Guard.MaxLength(trimmedDescription, Constants.DescriptionMaxLength, "Description");

        Id = id;
        CardId = cardId;
        Description = trimmedDescription;
        TransactionDate = transactionDate;
        Amount = amount;
    }
}
