# Negotiation System Documentation

## Overview

The Negotiation System handles customer interactions and item transactions in the pawn shop. It manages the buying process, price negotiations, and customer behavior.

## Architecture

### Core Components

- **`ICustomerService`** - Interface for customer management
- **`CustomerService`** - Implementation of customer service
- **`INegotiationHistoryService`** - Interface for negotiation history
- **`NegotiationHistoryService`** - Implementation of negotiation history
- **`NegotiationController`** - UI controller for negotiation interface

### Data Flow

```
Customer Generation → Customer Service → UI Controller
       ↓                    ↓              ↓
   Random Customer    Customer State    User Interface
```

## Negotiation Mechanics

### Buying Only

The system currently supports only buying items from customers:

- **No Selling**: Players cannot sell items to customers
- **Customer Items**: Customers bring items to sell to the player
- **Player as Buyer**: Player acts as the pawn shop owner

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

### With Customer Factory

```csharp
// In GameLoopState.ShowNextCustomer()
var customer = _customerFactory.GenerateRandomCustomer();
_customerService.SetCurrent(customer);
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
    _customerService.OnCustomerChanged += OnCustomerChanged;
    ShowNextCustomer();
}
```

- **Event Subscription**: Listens for customer events
- **Customer Flow**: Manages customer progression
- **State Transitions**: Handles game state changes

### Customer Progression

```csharp
// In GameLoopState.ShowNextCustomer()
private void ShowNextCustomer()
{
    var customer = _customerFactory.GenerateRandomCustomer();
    _customerService.SetCurrent(customer);
}
```

- **Automatic Generation**: Creates new customers automatically
- **Service Integration**: Uses customer service for customer management
- **Continuous Flow**: Maintains game progression
