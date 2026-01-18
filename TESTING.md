# Wex CLI - Sample Commands

## Running the Application

```bash
# Interactive mode
dotnet run --project src/Wex.Cli

# Single command mode
dotnet run --project src/Wex.Cli -- card create --number 1234567890123456 --limit 5000
```

## Card Commands

### Create a Card

```bash
card create --number 1234567890123456 --limit 5000
card create --number 9876543210123456 --limit 10000
card create --number 1111222233334444 --limit 2500.50
```

### Get Card Balance

```bash
# Balance in USD (default)
card balance --card-number 1234567890123456

# Balance converted to another currency
card balance --card-number 1234567890123456 --currency EUR
card balance --card-number 1234567890123456 --currency GBP
card balance --card-number 1234567890123456 --currency CAD

# Balance as of a specific date
card balance --card-number 1234567890123456 --currency EUR --as-of 2025-01-15
```

## Purchase Commands

### Add a Purchase

```bash
purchase add --card-number 1234567890123456 --description "Coffee" --date 2025-01-15 --amount 4.50
purchase add --card-number 1234567890123456 --description "Groceries" --date 2025-01-16 --amount 125.99
purchase add --card-number 1234567890123456 --description "Gas" --date 2025-01-17 --amount 45.00
purchase add --card-number 1234567890123456 --description "Restaurant dinner" --date 2025-01-18 --amount 85.50
```

### Get Purchase with Currency Conversion

```bash
# Get purchase in USD (default) - use the purchase ID returned when adding
purchase get --purchase-id <purchase-id>

# Get purchase converted to another currency
purchase get --purchase-id <purchase-id> --currency EUR
purchase get --purchase-id <purchase-id> --currency GBP
purchase get --purchase-id <purchase-id> --currency JPY
```

## Sample Test Session

```bash
# Start interactive mode
wex> help

# Create a card
wex> card create --number 4111111111111111 --limit 5000

# Add some purchases (note the Purchase ID returned)
wex> purchase add --card-number 4111111111111111 --description "Amazon order" --date 2025-01-10 --amount 150.00
wex> purchase add --card-number 4111111111111111 --description "Netflix subscription" --date 2025-01-15 --amount 15.99
wex> purchase add --card-number 4111111111111111 --description "Uber ride" --date 2025-01-18 --amount 25.50

# Check balance
wex> card balance --card-number 4111111111111111
wex> card balance --card-number 4111111111111111 --currency EUR

# Get purchase details with conversion (use actual purchase ID from above)
wex> purchase get --purchase-id <purchase-id> --currency GBP

# Exit
wex> exit
```

## Error Scenarios to Test

```bash
# Invalid card number (not 16 digits)
card create --number 123 --limit 1000

# Duplicate card number
card create --number 1234567890123456 --limit 1000
card create --number 1234567890123456 --limit 2000

# Non-existent card
card balance --card-number 9999999999999999

# Purchase exceeds credit limit
card create --number 5555666677778888 --limit 100
purchase add --card-number 5555666677778888 --description "Big purchase" --date 2025-01-18 --amount 150

# Description too long (max 50 characters)
purchase add --card-number 1234567890123456 --description "This description is way too long and exceeds the maximum allowed length" --date 2025-01-18 --amount 10

# Invalid currency
purchase get --purchase-id <purchase-id> --currency INVALID

# Purchase date too old for exchange rate (more than 6 months)
purchase add --card-number 1234567890123456 --description "Old purchase" --date 2020-01-01 --amount 50
```

## Supported Currencies

The application uses the Treasury Reporting Rates of Exchange API. Common currencies include:

- EUR (Euro)
- GBP (British Pound)
- CAD (Canadian Dollar)
- JPY (Japanese Yen)
- AUD (Australian Dollar)
- CHF (Swiss Franc)
- MXN (Mexican Peso)
- CNY (Chinese Yuan)

For a full list, see: https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange
