using Wex.Domain.Entities;

namespace Wex.Domain.Abstractions;

public interface IPurchaseRepository
{
    Task<PurchaseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PurchaseTransaction>> ListByCardIdAsync(Guid cardId, CancellationToken cancellationToken);
    Task AddAsync(PurchaseTransaction purchase, CancellationToken cancellationToken);
}
