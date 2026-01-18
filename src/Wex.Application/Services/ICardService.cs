using System.Threading;
using System.Threading.Tasks;
using Wex.Application.Models;

namespace Wex.Application.Services;

public interface ICardService
{
    Task<CardCreated> CreateAsync(string cardNumber, decimal creditLimitUsd, CancellationToken cancellationToken);
    Task<CardBalance> GetBalanceAsync(string cardNumber, string currency, DateOnly asOfDate, CancellationToken cancellationToken);
}
