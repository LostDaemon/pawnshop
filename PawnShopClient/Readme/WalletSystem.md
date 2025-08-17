# Wallet System Documentation

## Overview

The Wallet System manages the player's financial resources and handles all monetary transactions in the pawn shop. It provides a simple interface for checking balances and processing transactions.

## Architecture

### Core Components

- **`IWalletService`** - Interface defining wallet operations
- **`WalletService`** - Main implementation of wallet functionality
- **`MoneyLabelController`** - UI controller for displaying current balance

### Data Flow

```
Transaction Request → Wallet Service → Balance Update → UI Update
       ↓                    ↓              ↓            ↓
   Service Call        Validation      Balance      Display
```

## Financial Structure

### Currency System

```csharp
public enum CurrencyType
{
    Money = 0
}
```

- **Single Currency**: Only Money currency type implemented
- **Integer Values**: All monetary amounts stored as long integers
- **No Decimals**: Currency system uses whole numbers only

### Account Management

```csharp
public class WalletService : IWalletService
{
    private long _balance = 1000; // Starting balance
}
```

- **Starting Balance**: Player begins with 1000 money units
- **Balance Storage**: Current balance stored in private field
- **Simple Structure**: No multiple accounts or complex financial instruments

## Transaction Types

### Item Sales

```csharp
// In SellService.SellScheduledItem()
_wallet.TransactionAttempt(CurrencyType.Money, sellPrice);
```

- **Sale Proceeds**: Money added to wallet when items are sold
- **Automatic Processing**: Sales happen automatically at scheduled times
- **Price Calculation**: Sale price determined by item properties

### Expense Transactions

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
```

- **Item Purchases**: Money deducted from wallet when buying items
- **Negative Amounts**: Purchase transactions use negative values
- **Validation**: Transaction only succeeds if sufficient funds available

## Core Operations

### Balance Management

```csharp
public interface IWalletService
{
    long GetBalance(CurrencyType currencyType);
    bool TransactionAttempt(CurrencyType currencyType, long amount);
}
```

- **Balance Retrieval**: Get current balance for specific currency
- **Transaction Processing**: Attempt to modify balance by specified amount
- **Success Validation**: Return boolean indicating transaction success

### Transaction Processing

```csharp
public bool TransactionAttempt(CurrencyType currencyType, long amount)
{
    if (currencyType != CurrencyType.Money)
        return false;

    long newBalance = _balance + amount;
    if (newBalance < 0)
        return false;

    _balance = newBalance;
    return true;
}
```

- **Currency Validation**: Only Money currency type supported
- **Overdraft Prevention**: Transactions fail if result would be negative
- **Atomic Updates**: Balance updated only if transaction succeeds

## Integration Points

### With Negotiation System

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
if (success)
{
    CurrentItem.PurchasePrice = offeredPrice;
    _inventory.Put(CurrentItem);
    OnPurchased?.Invoke(CurrentItem);
}
```

- **Purchase Validation**: Check if player can afford items
- **Transaction Processing**: Handle money transfer for purchases
- **Success Handling**: Only proceed with purchase if transaction succeeds

### With Sell System

```csharp
// In SellService.SellScheduledItem()
var sellPrice = CalculateSellPrice(item);
_wallet.TransactionAttempt(CurrencyType.Money, sellPrice);
```

- **Sale Processing**: Add money to wallet when items are sold
- **Price Integration**: Use calculated sale price for transaction
- **Automatic Revenue**: Sales generate income automatically

## User Interface

### Money Display

```csharp
public class MoneyLabelController : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [Inject] private IWalletService _wallet;

    private void Start()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_label != null)
        {
            _label.text = $"${_wallet.GetBalance(CurrencyType.Money)}";
        }
    }
}
```

- **Real-Time Updates**: Display shows current balance
- **Currency Formatting**: Money displayed with dollar sign
- **Service Integration**: Uses WalletService for balance data

### UI Updates

```csharp
// In MoneyLabelController
private void UpdateDisplay()
{
    if (_label != null)
    {
        _label.text = $"${_wallet.GetBalance(CurrencyType.Money)}";
    }
}
```

- **Dynamic Content**: UI reflects current wallet state
- **Simple Format**: Basic text display without complex formatting
- **Direct Access**: Controller directly queries wallet service

## Service Registration

### Dependency Injection

```csharp
// In ProjectInstaller.InstallBindings()
Container.Bind<IWalletService>()
    .To<WalletService>()
    .AsSingle();
```

- **Service Registration**: Wallet registered in dependency injection
- **Singleton Pattern**: Single instance of wallet service
- **Interface Binding**: Service bound to IWalletService interface

## Transaction Flow

### Purchase Flow

1. **Negotiation**: Player negotiates item price with customer
2. **Validation**: Check if player can afford the item
3. **Transaction**: Deduct money from wallet if sufficient funds
4. **Item Transfer**: Move item to player inventory
5. **Balance Update**: UI reflects new wallet balance

### Sale Flow

1. **Scheduling**: Item scheduled for sale with time delay
2. **Automatic Sale**: Sale occurs automatically at scheduled time
3. **Price Calculation**: Final sale price determined
4. **Transaction**: Add money to wallet
5. **Balance Update**: UI reflects new wallet balance

## Usage Examples

### Basic Balance Check

```csharp
// Get current balance
long currentBalance = walletService.GetBalance(CurrencyType.Money);

// Check if can afford item
bool canAfford = currentBalance >= itemPrice;
```

### Transaction Processing

```csharp
// Purchase item
bool purchaseSuccess = walletService.TransactionAttempt(CurrencyType.Money, -itemPrice);
if (purchaseSuccess)
{
    // Item purchased successfully
    Debug.Log($"Purchased item for ${itemPrice}");
}
else
{
    // Insufficient funds
    Debug.Log("Not enough money to purchase item");
}

// Sell item
bool saleSuccess = walletService.TransactionAttempt(CurrencyType.Money, salePrice);
if (saleSuccess)
{
    // Item sold successfully
    Debug.Log($"Sold item for ${salePrice}");
}
```

### UI Integration

```csharp
// In MoneyLabelController
private void UpdateDisplay()
{
    if (_label != null)
    {
        long balance = _wallet.GetBalance(CurrencyType.Money);
        _label.text = $"${balance}";
    }
}

// Call this method whenever balance changes
public void OnBalanceChanged()
{
    UpdateDisplay();
}
```
