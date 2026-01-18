# Wex Coding Exercise

Console application that manages cards and purchase transactions with currency conversion using the
U.S. Treasury Reporting Rates of Exchange API.

## CLI Usage

Build:

```bash
dotnet build Wex.slnx
```

Create a card:

```bash
dotnet run --project src/Wex.Cli -- card create --number 1234567890123456 --limit 5000.00
```

Record a purchase:

```bash
dotnet run --project src/Wex.Cli -- purchase add --card-id <card-guid> --description "Fuel" --date 2024-01-15 --amount 125.50
```

Retrieve a converted purchase:

```bash
dotnet run --project src/Wex.Cli -- purchase get --purchase-id <purchase-guid> --currency EUR
```

Retrieve available balance:

```bash
dotnet run --project src/Wex.Cli -- card balance --card-id <card-guid> --currency EUR --as-of 2024-01-31
```

## Configuration

Configuration is stored in `src/Wex.Cli/appsettings.json`. The default SQLite file is `./data/wex.db`
relative to the CLI working directory.
