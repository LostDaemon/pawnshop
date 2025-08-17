# Negotiation System Documentation

## Overview

The Negotiation System handles customer interactions and item transactions in the pawn shop. It manages the buying process, price negotiations, and customer behavior.

## Architecture

### Core Components

- **`INegotiationService`** - Interface defining negotiation operations
- **`NegotiationService`** - Main implementation of negotiation logic
- **`ICustomerService`** - Interface for customer management
- **`CustomerService`** - Implementation of customer service
- **`INegotiationHistoryService`** - Interface for negotiation history
- **`NegotiationHistoryService`** - Implementation of negotiation history
- **`NegotiationController`** - UI controller for negotiation interface

### Data Flow

```
Customer Generation → Negotiation Service → Customer Service → UI Controller
       ↓                    ↓                    ↓              ↓
   Random Customer    Price Logic        Customer State    User Interface
```

## Negotiation Mechanics

### Buying Only

The system currently supports only buying items from customers:

- **No Selling**: Players cannot sell items to customers
- **Customer Items**: Customers bring items to sell to the player
- **Player as Buyer**: Player acts as the pawn shop owner

### Price Haggling

```csharp
// In NegotiationService.TryCounterOffer()
public bool TryCounterOffer(long playerOffer)
{
    var customer = _customerService.Current;
    var minAcceptable = (long)(CurrentItem.BasePrice * 0.6f * (1f - customer.UncertaintyLevel));
    var maxAcceptable = (long)(CurrentItem.BasePrice * 0.95f);

    if (playerOffer < minAcceptable || playerOffer > maxAcceptable)
        return false;

    float t = (float)(playerOffer - minAcceptable) / (maxAcceptable - minAcceptable);
    float chance = Mathf.Lerp(0.9f, 0.1f, t);
    return _random.NextDouble() < chance;
}
```

- **Price Range**: Customer accepts offers between 60-95% of base price
- **Uncertainty Factor**: Customer uncertainty affects minimum acceptable price
- **Acceptance Probability**: Higher offers have lower acceptance chance

## Price Calculation

### Initial NPC Offer

```csharp
// In NegotiationService.GenerateInitialNpcOffer()
private void GenerateInitialNpcOffer()
{
    var basePrice = CurrentItem.BasePrice;
    var randomFactor = _random.Next(60, 86); // 60-85%
    CurrentNpcOffer = (long)(basePrice * randomFactor / 100f);
}
```

- **Base Price**: Starting point from ItemPrototype
- **Random Range**: 60-85% of base price
- **Initial Offer**: Customer's first price proposal

### Counter-Offer System

```csharp
// In NegotiationService.TryCounterOffer()
var minAcceptable = (long)(CurrentItem.BasePrice * 0.6f * (1f - customer.UncertaintyLevel));
var maxAcceptable = (long)(CurrentItem.BasePrice * 0.95f);
```

- **Minimum Acceptable**: 60% of base price, reduced by customer uncertainty
- **Maximum Acceptable**: 95% of base price
- **Uncertainty Impact**: Higher uncertainty lowers minimum acceptable price

## Customer System

### Customer Properties

```csharp
public class Customer
{
    public ItemModel OwnedItem { get; set; }
    public float UncertaintyLevel { get; set; }
    public float Mood { get; set; }
}
```

- **Owned Item**: Item the customer wants to sell
- **Uncertainty Level**: Affects price negotiation (0.0 to 1.0)
- **Mood**: Customer's current emotional state

### Customer Behavior

```csharp
// In NegotiationService.QuestionItemOrigin()
public void QuestionItemOrigin()
{
    var customer = _customerService.Current;
    customer.UncertaintyLevel += 0.25f;
    customer.Mood -= 0.1f;

    _history.Add(new TextRecord(HistoryRecordSource.Customer, "Umm... I'm not sure where this item came from."));
}
```

- **Origin Questioning**: Increases customer uncertainty
- **Mood Changes**: Affects customer emotional state
- **Behavioral Response**: Customer becomes more defensive

### Customer Service

```csharp
public interface ICustomerService
{
    Customer Current { get; }
    void SetCurrent(Customer customer);
}
```

- **Current Customer**: Tracks active customer
- **Customer Management**: Handles customer switching

## Negotiation Process

### Step-by-Step Process

1. **Customer Arrival**: Random customer generated with item
2. **Item Assessment**: Player sees item name and current offer
3. **Price Negotiation**: Player can make counter-offers
4. **Customer Response**: Customer accepts or rejects offers
5. **Transaction**: Successful negotiation leads to purchase

### Negotiation Options

```csharp
// In NegotiationController
public void OnAcceptButtonClicked()
{
    _negotiationService.AcceptCurrentOffer();
}

public void OnCounterOfferButtonClicked()
{
    _negotiationService.TryCounterOffer(_counterOfferInput.text);
}

public void OnRequestDiscountButtonClicked()
{
    _negotiationService.RequestDiscount();
}

public void OnQuestionOriginButtonClicked()
{
    _negotiationService.QuestionItemOrigin();
}

public void OnSkipButtonClicked()
{
    _negotiationService.SkipCurrentCustomer();
}
```

- **Accept Offer**: Purchase at current price
- **Counter Offer**: Propose different price
- **Request Discount**: Ask for 10% reduction
- **Question Origin**: Investigate item authenticity
- **Skip Customer**: Decline to negotiate

## Item Origin Investigation

### Authenticity Checking

```csharp
// In NegotiationService.QuestionItemOrigin()
public void QuestionItemOrigin()
{
    var customer = _customerService.Current;
    customer.UncertaintyLevel += 0.25f;
    customer.Mood -= 0.1f;

    _history.Add(new TextRecord(HistoryRecordSource.Customer, "Umm... I'm not sure where this item came from."));
}
```

- **Uncertainty Increase**: Customer becomes less confident
- **Mood Decrease**: Customer becomes defensive
- **Behavioral Clues**: Customer response indicates item authenticity

## UI Integration

### Negotiation Controller

```csharp
public class NegotiationController : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _currentOfferText;
    [SerializeField] private TMP_InputField _counterOfferInput;
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _counterOfferButton;
    [SerializeField] private Button _requestDiscountButton;
    [SerializeField] private Button _questionOriginButton;
    [SerializeField] private Button _skipButton;
}
```

- **Item Display**: Shows current item name and offer
- **Input Fields**: Allows player to enter counter-offers
- **Action Buttons**: Provides negotiation options
- **Real-Time Updates**: UI reflects current negotiation state

### UI Updates

```csharp
// In NegotiationController.OnCurrentItemChanged()
private void OnCurrentItemChanged(ItemModel item)
{
    if (item != null)
    {
        _itemNameText.text = item.Name;
        UpdateOfferDisplay();
        UpdateButtonStates();
    }
}
```

- **Dynamic Content**: UI updates based on current item
- **Button States**: Buttons enabled/disabled based on context
- **Offer Display**: Shows current negotiation price

## Transaction Processing

### Purchase Logic

```csharp
// In NegotiationService.TryPurchase()
public bool TryPurchase(long offeredPrice)
{
    var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
    if (success)
    {
        CurrentItem.PurchasePrice = offeredPrice;
        _inventory.Put(CurrentItem);
        OnPurchased?.Invoke(CurrentItem);
    }
    return success;
}
```

- **Fund Validation**: Checks if player can afford item
- **Transaction Processing**: Deducts money from wallet
- **Item Transfer**: Moves item to player inventory
- **Event Notification**: Notifies subscribers of successful purchase

### Transaction Events

```csharp
public event Action<ItemModel> OnPurchased;
public event Action<ItemModel> OnCurrentItemChanged;
public event Action OnSkipRequested;
```

- **Purchase Success**: Fired when item is successfully bought
- **Item Changes**: Fired when current item changes
- **Skip Request**: Fired when player wants to skip customer

## History System

### Negotiation Records

```csharp
// In NegotiationHistoryService
public void AddRecord(TextRecord record)
{
    _history.Add(record);
    OnHistoryChanged?.Invoke();
}
```

- **Record Storage**: Tracks negotiation conversations
- **Event Notifications**: Notifies when history changes
- **Persistent Data**: Maintains conversation history

### Record Types

```csharp
public class TextRecord
{
    public HistoryRecordSource Source;
    public string Text;
    public DateTime Timestamp;
}
```

- **Source Identification**: Distinguishes between player and customer
- **Text Content**: Actual conversation content
- **Timestamp**: When the record was created

## Integration Points

### With Wallet System

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
```

- **Balance Checking**: Validates player can afford items
- **Transaction Processing**: Handles money transfer
- **Financial Integration**: Links negotiation to financial system

### With Inventory System

```csharp
// In NegotiationService.TryPurchase()
_inventory.Put(CurrentItem);
```

- **Item Storage**: Purchased items go to inventory
- **Storage Management**: Integrates with storage system
- **Item Tracking**: Maintains item ownership

### With Customer Factory

```csharp
// In GameLoopState.ShowNextCustomer()
var customer = _customerFactory.GenerateRandomCustomer();
_negotiationService.SetCurrentCustomer(customer);
```

- **Customer Generation**: Creates random customers with items
- **Game Flow**: Integrates with main game loop
- **Customer Management**: Handles customer switching

## Game Loop Integration

### State Management

```csharp
// In GameLoopState
public void Enter()
{
    _purchaseService.OnPurchased += OnItemPurchased;
    _purchaseService.OnSkipRequested += ShowNextCustomer;
    ShowNextCustomer();
}
```

- **Event Subscription**: Listens for negotiation events
- **Customer Flow**: Manages customer progression
- **State Transitions**: Handles game state changes

### Customer Progression

```csharp
// In GameLoopState.ShowNextCustomer()
private void ShowNextCustomer()
{
    var customer = _customerFactory.GenerateRandomCustomer();
    _purchaseService.SetCurrentCustomer(customer);
}
```

- **Automatic Generation**: Creates new customers automatically
- **Service Integration**: Uses negotiation service for customer management
- **Continuous Flow**: Maintains game progression
