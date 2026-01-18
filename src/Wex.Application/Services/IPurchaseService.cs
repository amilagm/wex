using System;
using System.Threading;
using System.Threading.Tasks;
using Wex.Application.Models;

namespace Wex.Application.Services;

public interface IPurchaseService
{
    Task<PurchaseCreated> AddAsync(string cardNumber, string description, DateOnly transactionDate, decimal amountUsd, CancellationToken cancellationToken);
    Task<ConvertedPurchase> GetConvertedAsync(Guid purchaseId, string currency, CancellationToken cancellationToken);
}
