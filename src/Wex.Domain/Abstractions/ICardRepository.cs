using Wex.Domain.Entities;

namespace Wex.Domain.Abstractions;

public interface ICardRepository
{
    Task<Card?> GetByNumberAsync(string number, CancellationToken cancellationToken);
    Task AddAsync(Card card, CancellationToken cancellationToken);
}
