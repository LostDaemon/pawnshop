# Item Workflow Scenario Documentation

## Overview

The Item Workflow Scenario describes the complete lifecycle of items in the pawn shop, from customer arrival to final sale. It covers the acquisition, processing, management, and sale phases of item handling.

## Core Workflow Phases

### 1. Acquisition Phase

#### Customer Generation

```csharp
// In GameLoopState.ShowNextCustomer()
var customer = _customerFactory.GenerateRandomCustomer();
_purchaseService.SetCurrentCustomer(customer);
```

- **Random Generation**: CustomerFactory creates random customers
- **Item Assignment**: Each customer has a specific item to sell
- **Service Integration**: Customer assigned to negotiation service

#### Item Assessment

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
- **Initial Offer**: Customer's first price proposal (60-85% of base)
- **Price Range**: Determines negotiation boundaries

### 2. Processing Phase

#### Item Inspection

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

- **Authenticity Check**: Questioning item origin reveals customer uncertainty
- **Behavioral Analysis**: Customer responses indicate item authenticity
- **Mood Impact**: Investigation affects customer emotional state

#### Negotiation Options

```csharp
// In NegotiationController
public void OnAcceptButtonClicked() => _negotiationService.AcceptCurrentOffer();
public void OnCounterOfferButtonClicked() => _negotiationService.TryCounterOffer(_counterOfferInput.text);
public void OnRequestDiscountButtonClicked() => _negotiationService.RequestDiscount();
public void OnQuestionOriginButtonClicked() => _negotiationService.QuestionItemOrigin();
public void OnSkipButtonClicked() => _negotiationService.SkipCurrentCustomer();
```

- **Accept Offer**: Purchase at current price
- **Counter Offer**: Propose different price
- **Request Discount**: Ask for price reduction
- **Question Origin**: Investigate item authenticity
- **Skip Customer**: Decline to negotiate

### 3. Management Phase

#### Purchase Processing

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

- **Fund Validation**: Check if player can afford item
- **Transaction Processing**: Deduct money from wallet
- **Item Transfer**: Move item to player inventory
- **Price Recording**: Store purchase price on item

#### Inventory Storage

```csharp
// In NegotiationService.TryPurchase()
_inventory.Put(CurrentItem);
```

- **Storage Assignment**: Item placed in player inventory
- **Ownership Transfer**: Item now belongs to player
- **Access Management**: Item available for further processing

### 4. Sale Phase

#### Sale Scheduling

```csharp
// In SellService.ScheduleForSale()
public void ScheduleForSale(ItemModel item)
{
    if (!_inventory.Withdraw(item))
        return false;

    _sellStorage.Put(item);

    int delayHours = UnityEngine.Random.Range(1, 4);
    var scheduleTime = AddHours(_timeService.CurrentTime, delayHours);
    _timeService.Schedule(scheduleTime, () => SellScheduledItem(item));

    return true;
}
```

- **Inventory Withdrawal**: Item removed from player inventory
- **Sale Storage**: Item placed in sell storage
- **Time Scheduling**: Sale scheduled for 1-4 hours later
- **Automatic Processing**: Sale occurs automatically at scheduled time

#### Automatic Sale

```csharp
// In SellService.SellScheduledItem()
private void SellScheduledItem(ItemModel item)
{
    if (!_sellStorage.Withdraw(item))
        return;

    var sellPrice = CalculateSellPrice(item);
    _wallet.TransactionAttempt(CurrencyType.Money, sellPrice);

    OnItemSold?.Invoke(item, sellPrice);
}
```

- **Storage Removal**: Item removed from sell storage
- **Price Calculation**: Final sale price determined
- **Money Addition**: Sale proceeds added to wallet
- **Event Notification**: Sale completion event fired

## Skill Integration Points

### Current Integration

- **Inspection Skills**: Required for revealing hidden item properties
- **Negotiation Skills**: Affect price negotiation outcomes
- **Knowledge Skills**: Required for understanding item details

### Future Integration

- **Restoration Skills**: May affect item condition improvement
- **Expertise Levels**: Advanced skills provide better item assessment
- **Specialization**: Skills may unlock specific item categories

## Decision Points and Consequences

### Purchase Decisions

- **Accept Current Offer**: Immediate purchase at customer's price
- **Make Counter Offer**: Risk rejection for potential savings
- **Request Discount**: Attempt to reduce price by 10%
- **Question Origin**: Reveal item authenticity but affect customer mood
- **Skip Customer**: Lose opportunity but avoid unwanted items

### Sale Decisions

- **Immediate Sale**: Quick cash but potentially lower profit
- **Scheduled Sale**: Higher profit but delayed cash flow
- **Item Selection**: Choose which items to prioritize for sale

## Error Handling and Edge Cases

### Insufficient Funds

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
if (!success)
{
    // Player cannot afford the item
    return false;
}
```

- **Fund Validation**: Check affordability before purchase
- **Graceful Failure**: Return false without throwing exceptions
- **User Feedback**: UI can show insufficient funds message

### Item Availability

```csharp
// In SellService.ScheduleForSale()
if (!_inventory.Withdraw(item))
    return false;
```

- **Inventory Check**: Verify item exists in inventory
- **Safe Withdrawal**: Only proceed if item is available
- **Error Prevention**: Avoid null reference exceptions

## Performance Metrics

### Transaction Speed

- **Purchase Time**: Time from customer arrival to purchase completion
- **Sale Time**: Time from scheduling to automatic sale
- **Processing Efficiency**: Items processed per game session

### Financial Performance

- **Purchase Cost**: Total money spent on items
- **Sale Revenue**: Total money earned from sales
- **Profit Margin**: Difference between purchase and sale prices

## Workflow Optimization

### Current Optimizations

- **Automatic Scheduling**: Sales happen automatically without player input
- **Random Delays**: 1-4 hour delays prevent immediate profit exploitation
- **Batch Processing**: Multiple items can be scheduled simultaneously

### System Efficiency

- **Event-Driven**: UI updates automatically on state changes
- **Service Separation**: Clear separation of concerns between systems
- **Dependency Injection**: Services easily testable and replaceable

## Integration with Other Systems

### Time System

```csharp
// In SellService.ScheduleForSale()
var scheduleTime = AddHours(_timeService.CurrentTime, delayHours);
_timeService.Schedule(scheduleTime, () => SellScheduledItem(item));
```

- **Time Scheduling**: Uses TimeService for sale scheduling
- **Automatic Execution**: Sales occur at specific game times
- **Event Coordination**: Time system triggers sale events

### Wallet System

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);

// In SellService.SellScheduledItem()
_wallet.TransactionAttempt(CurrencyType.Money, sellPrice);
```

- **Purchase Transactions**: Deducts money for item purchases
- **Sale Transactions**: Adds money from item sales
- **Balance Management**: Maintains player's financial state

### Storage System

```csharp
// In NegotiationService.TryPurchase()
_inventory.Put(CurrentItem);

// In SellService.ScheduleForSale()
_inventory.Withdraw(item);
_sellStorage.Put(item);
```

- **Inventory Management**: Tracks player-owned items
- **Sale Storage**: Manages items scheduled for sale
- **Item Movement**: Handles item transfers between storage types
