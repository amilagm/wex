# Card Management

Console application that manages cards and purchase transactions with currency conversion using the U.S. Treasury Reporting Rates of Exchange API.

## Requirements

- .NET 10 SDK

## Build

```bash
dotnet build
```

## Run Tests

```bash
dotnet test
```

## Usage

Start the interactive CLI:

```bash
dotnet run --project src/Wex.Cli
```

### Interactive Commands

Once running, you'll see the `wex>` prompt. Available commands:

**Card Management**
```
card create --number <16-digit-number> --limit <amount>
card balance --card-number <16-digit-number> [--currency <code>] [--as-of <date>]
```

**Purchase Management**
```
purchase add --card-number <16-digit-number> --description <text> --date <yyyy-MM-dd> --amount <amount>
purchase get --purchase-id <guid> [--currency <code>]
```

**Other**
```
help     Show available commands
exit     Quit the application
```

### Example Session

```
wex> card create --number 1234567890123456 --limit 5000
Card created successfully
Card number: 1234567890123456
Credit limit (USD): 5000.00

wex> purchase add --card-number 1234567890123456 --description "Fuel" --date 2024-01-15 --amount 125.50
Purchase recorded successfully
Purchase ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6
Card number: 1234567890123456
Description: Fuel
Date: 2024-01-15
Amount (USD): 125.50

wex> card balance --card-number 1234567890123456 --currency EUR
Card number: 1234567890123456
Credit limit (USD): 5000.00
Total purchases (USD): 125.50
Available (USD): 4874.50
Exchange rate (EUR): 0.92 on 2024-01-15
Available (EUR): 4484.54

wex> exit
Goodbye!
```

## Configuration

Configuration is stored in `src/Wex.Cli/appsettings.json`. The SQLite database is stored in the `data/` folder at the solution root.

## Direct Command Execution

Commands can also be passed directly for scripting:

```bash
dotnet run --project src/Wex.Cli -- card create --number 1234567890123456 --limit 5000.00
dotnet run --project src/Wex.Cli -- purchase add --card-number 1234567890123456 --description "Fuel" --date 2024-01-15 --amount 125.50
dotnet run --project src/Wex.Cli -- purchase get --purchase-id <guid> --currency EUR
dotnet run --project src/Wex.Cli -- card balance --card-number 1234567890123456 --currency EUR --as-of 2024-01-31
```
