using Wex.Domain.Abstractions;
using Wex.Domain.Entities;

namespace Wex.Tests.Support;

public sealed class InMemoryCardRepository : ICardRepository
{
    private readonly List<Card> _cards = new();

    public Task<Card?> GetByNumberAsync(string number, CancellationToken cancellationToken)
    {
        return Task.FromResult(_cards.SingleOrDefault(c => c.Number == number));
    }

    public Task AddAsync(Card card, CancellationToken cancellationToken)
    {
        _cards.Add(card);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryPurchaseRepository : IPurchaseRepository
{
    private readonly List<PurchaseTransaction> _purchases = new();

    public Task<PurchaseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_purchases.SingleOrDefault(p => p.Id == id));
    }

    public Task<IReadOnlyList<PurchaseTransaction>> ListByCardIdAsync(Guid cardId, CancellationToken cancellationToken)
    {
        IReadOnlyList<PurchaseTransaction> result = _purchases.Where(p => p.CardId == cardId).ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(PurchaseTransaction purchase, CancellationToken cancellationToken)
    {
        _purchases.Add(purchase);
        return Task.CompletedTask;
    }
}
