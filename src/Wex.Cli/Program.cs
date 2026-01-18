using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Wex.Application;
using Wex.Cli.Commands;
using Wex.DataAccess;
using Wex.DataAccess.Database;
using Wex.Domain.Exceptions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/wex.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true);

builder.Services.AddSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICliCommand, CardCommands>();
builder.Services.AddScoped<ICliCommand, PurchaseCommands>();
builder.Services.AddScoped<CommandBuilder>();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    await host.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync(CancellationToken.None);

    var rootCommand = host.Services.GetRequiredService<CommandBuilder>().Build();

    if (args.Length > 0)
    {
        return await rootCommand.Parse(args).InvokeAsync();
    }

    return await RunInteractiveMode(rootCommand);
}
catch (WexException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
catch (Exception ex)
{
    logger.LogError(ex, "An unexpected error occurred");
    Console.Error.WriteLine("An unexpected error occurred. Please try again later.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

async Task<int> RunInteractiveMode(System.CommandLine.RootCommand rootCommand)
{
    Console.WriteLine("Wex - Credit card and purchase management CLI");
    Console.WriteLine("Type 'help' for available commands, 'exit' to quit.");
    Console.WriteLine();

    while (true)
    {
        Console.Write("wex> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            continue;

        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Goodbye!");
            return 0;
        }

        if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            input = "--help";
        }

        await rootCommand.Parse(input).InvokeAsync();

        Console.WriteLine();
    }
}
