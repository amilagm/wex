using System.Text.RegularExpressions;
using Wex.Domain.Exceptions;
using Wex.Domain.ValueObjects;

namespace Wex.Domain.Entities;

public class Card
{
    private static readonly Regex CardNumberPattern = new("^[0-9]{16}$", RegexOptions.Compiled);

    public Guid Id { get; }
    public string Number { get; }
    public Money CreditLimit { get; }

    public Card(Guid id, string number, Money creditLimit)
    {
        Guard.NotEmpty(id, "Card ID");
        Guard.NotNullOrWhiteSpace(number, "Card number");

        if (!CardNumberPattern.IsMatch(number))
        {
            throw new DomainException("Card number must be a 16-digit numeric string.");
        }

        Id = id;
        Number = number;
        CreditLimit = creditLimit;
    }
}
