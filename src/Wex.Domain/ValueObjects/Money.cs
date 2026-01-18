using Wex.Domain.Exceptions;

namespace Wex.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Usd(decimal amount) => From(amount, Constants.BaseCurrency);

    public static Money From(decimal amount, string currency)
    {
        Guard.NotNullOrWhiteSpace(currency, "Currency");

        if (amount <= 0)
        {
            throw new DomainException("Amount must be a positive value.");
        }

        var rounded = Round(amount);
        return new Money(rounded, currency.ToUpperInvariant());
    }

    public static decimal Round(decimal amount) =>
        Math.Round(amount, Constants.MoneyDecimalPlaces, MidpointRounding.AwayFromZero);
}
