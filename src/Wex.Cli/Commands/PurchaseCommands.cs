using System.CommandLine;
using Wex.Application.Services;

namespace Wex.Cli.Commands;

public class PurchaseCommands(IPurchaseService purchaseService) : ICliCommand
{
    public Command Build()
    {
        var addCommand = new Command("add", "Add a new purchase");
        var cardNumberOption = new Option<string>("--card-number") { Description = "16-digit card number", Required = true };
        var descriptionOption = new Option<string>("--description") { Description = "Purchase description", Required = true };
        var dateOption = new Option<DateOnly>("--date") { Description = "Transaction date (yyyy-MM-dd)", Required = true };
        var amountOption = new Option<decimal>("--amount") { Description = "Amount in USD", Required = true };
        addCommand.Add(cardNumberOption);
        addCommand.Add(descriptionOption);
        addCommand.Add(dateOption);
        addCommand.Add(amountOption);
        addCommand.SetHandledAction(async (parseResult, ct) =>
        {
            var purchase = await purchaseService.AddAsync(
                parseResult.GetValue(cardNumberOption)!,
                parseResult.GetValue(descriptionOption)!,
                parseResult.GetValue(dateOption),
                parseResult.GetValue(amountOption),
                ct);

            Console.WriteLine($"Purchase recorded successfully");
            Console.WriteLine($"Purchase ID: {purchase.Id}");
            Console.WriteLine($"Card number: {parseResult.GetValue(cardNumberOption)}");
            Console.WriteLine($"Description: {purchase.Description}");
            Console.WriteLine($"Date: {purchase.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine($"Amount (USD): {purchase.AmountUsd:0.00}");
        });

        var getCommand = new Command("get", "Get purchase details");
        var purchaseIdOption = new Option<Guid>("--purchase-id") { Description = "Purchase ID", Required = true };
        var currencyOption = new Option<string>("--currency") { Description = "Currency code for conversion", DefaultValueFactory = _ => "USD" };
        getCommand.Add(purchaseIdOption);
        getCommand.Add(currencyOption);
        getCommand.SetHandledAction(async (parseResult, ct) =>
        {
            var purchase = await purchaseService.GetConvertedAsync(
                parseResult.GetValue(purchaseIdOption),
                parseResult.GetValue(currencyOption)!,
                ct);

            Console.WriteLine($"Purchase: {purchase.Id}");
            Console.WriteLine($"Description: {purchase.Description}");
            Console.WriteLine($"Date: {purchase.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine($"Amount (USD): {purchase.AmountUsd:0.00}");
            Console.WriteLine($"Exchange rate ({purchase.ConvertedCurrency}): {purchase.UsdToTargetRate:0.######} on {purchase.ExchangeRateDate:yyyy-MM-dd}");
            Console.WriteLine($"Converted amount ({purchase.ConvertedCurrency}): {purchase.ConvertedAmount:0.00}");
        });

        return new Command("purchase", "Purchase management commands") { addCommand, getCommand };
    }
}
