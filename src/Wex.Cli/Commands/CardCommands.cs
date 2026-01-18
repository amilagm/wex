using System.CommandLine;
using Wex.Application.Services;

namespace Wex.Cli.Commands;

public class CardCommands(ICardService cardService) : ICliCommand
{
    public Command Build()
    {
        var createCommand = new Command("create", "Create a new card");
        var numberOption = new Option<string>("--number") { Description = "16-digit card number", Required = true };
        var limitOption = new Option<decimal>("--limit") { Description = "Credit limit in USD", Required = true };
        createCommand.Add(numberOption);
        createCommand.Add(limitOption);
        createCommand.SetHandledAction(async (parseResult, ct) =>
        {
            var card = await cardService.CreateAsync(
                parseResult.GetValue(numberOption)!,
                parseResult.GetValue(limitOption),
                ct);

            Console.WriteLine($"Card created successfully");
            Console.WriteLine($"Card number: {card.CardNumber}");
            Console.WriteLine($"Credit limit (USD): {card.CreditLimitUsd:0.00}");
        });

        var balanceCommand = new Command("balance", "Get card balance");
        var cardNumberOption = new Option<string>("--card-number") { Description = "16-digit card number", Required = true };
        var currencyOption = new Option<string>("--currency") { Description = "Currency code for conversion", DefaultValueFactory = _ => "USD" };
        var asOfOption = new Option<DateOnly?>("--as-of") { Description = "Date for balance calculation (yyyy-MM-dd)" };
        balanceCommand.Add(cardNumberOption);
        balanceCommand.Add(currencyOption);
        balanceCommand.Add(asOfOption);
        balanceCommand.SetHandledAction(async (parseResult, ct) =>
        {
            var cardNumber = parseResult.GetValue(cardNumberOption)!;
            var balance = await cardService.GetBalanceAsync(
                cardNumber,
                parseResult.GetValue(currencyOption)!,
                parseResult.GetValue(asOfOption) ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ct);

            Console.WriteLine($"Card number: {cardNumber}");
            Console.WriteLine($"Credit limit (USD): {balance.CreditLimitUsd:0.00}");
            Console.WriteLine($"Total purchases (USD): {balance.TotalPurchasesUsd:0.00}");
            Console.WriteLine($"Available (USD): {balance.AvailableUsd:0.00}");

            if (!string.Equals(balance.ConvertedCurrency, "USD", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Exchange rate ({balance.ConvertedCurrency}): {balance.UsdToTargetRate:0.######} on {balance.ExchangeRateDate:yyyy-MM-dd}");
                Console.WriteLine($"Available ({balance.ConvertedCurrency}): {balance.AvailableInTargetCurrency:0.00}");
            }
        });

        return new Command("card", "Card management commands") { createCommand, balanceCommand };
    }
}
