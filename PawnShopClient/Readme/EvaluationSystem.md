# Evaluation System Documentation

## Overview

The Evaluation System is responsible for determining the value of items based on their properties, tags, and the character performing the evaluation.

## Architecture

### Core Components

- **`IEvaluationService`** - Interface defining evaluation operations
- **`EvaluationService`** - Main implementation of item evaluation logic

### Data Flow

```
Item + Character → Evaluation Service → Price Calculation → Final Value
```

## Core Functionality

### Item Evaluation

The main evaluation method processes items and returns their estimated value:

```csharp
public interface IEvaluationService
{
    /// <summary>
    /// Evaluate an item and return it's estimated value
    /// </summary>
    /// <param name="character">Character performing the evaluation</param>
    /// <param name="item">Item to be evaluated</param>
    /// <returns>Estimated value of the item</returns>
    long Evaluate(ICharacter character, ItemModel item);
}
```

### Evaluation Process

The evaluation follows a systematic approach:

```csharp
public long Evaluate(ICharacter character, ItemModel item)
{
    if (item == null) return 0;

    long finalPrice = item.BasePrice;

    bool isPlayer = character is Player;
    bool isCustomer = character is Customer;

    if (item.Tags != null)
    {
        foreach (var tag in item.Tags)
        {
            bool isTagRevealed = false;
            if (isPlayer)
            {
                isTagRevealed = tag.IsRevealedToPlayer;
            }
            else if (isCustomer)
            {
                isTagRevealed = tag.IsRevealedToCustomer;
            }

            if (isTagRevealed)
            {
                finalPrice = (long)(finalPrice * tag.PriceMultiplier);
            }
        }
    }

    return Math.Max(0, finalPrice);
}
```

## Evaluation Logic

### Base Price Foundation

- **Starting Point**: Evaluation begins with the item's base price from `ItemModel.BasePrice`
- **Foundation Value**: Base price represents the item's fundamental market value

### Tag-Based Modifications

Tags influence final item value through multipliers:

```csharp
// Price calculation through tag multipliers
foreach (var tag in item.Tags)
{
    bool isTagRevealed = isPlayer ? tag.IsRevealedToPlayer : tag.IsRevealedToCustomer;

    if (isTagRevealed)
    {
        finalPrice = (long)(finalPrice * tag.PriceMultiplier);
    }
}
```

### Character-Specific Evaluation

Different characters see different values based on their knowledge:

#### Player Evaluation

```csharp
bool isPlayer = character is Player;
if (isPlayer)
{
    isTagRevealed = tag.IsRevealedToPlayer;
}
```

#### Customer Evaluation

```csharp
bool isCustomer = character is Customer;
if (isCustomer)
{
    isTagRevealed = tag.IsRevealedToCustomer;
}
```

### Price Calculation Formula

The final price is calculated as:

```
Final Price = Base Price × Tag1 Multiplier × Tag2 Multiplier × ... × TagN Multiplier
```

## Integration Points

### With Tag System

Tags provide the primary mechanism for value modification:

```csharp
// Tags affect item value through multipliers
if (isTagRevealed)
{
    finalPrice = (long)(finalPrice * tag.PriceMultiplier);
}
```

**Tag Properties Used:**

- **PriceMultiplier**: Direct multiplier for price calculation
- **IsRevealedToPlayer**: Whether player can see the tag
- **IsRevealedToCustomer**: Whether customer can see the tag

### With Item System

Items provide the foundation for evaluation:

```csharp
// Item properties used in evaluation
long finalPrice = item.BasePrice;  // Starting point
var tags = item.Tags;              // Value modifiers
```

**Item Properties:**

- **BasePrice**: Foundation value for calculations
- **Tags**: Collection of value-affecting properties

## Usage Examples

### Basic Item Evaluation

```csharp
// Evaluate an item as a player
var playerEvaluation = evaluationService.Evaluate(player, item);

// Evaluate the same item as a customer
var customerEvaluation = evaluationService.Evaluate(customer, item);
```

### Tag-Based Value Changes

```csharp
// Item with condition tag
var damagedItem = new ItemModel { BasePrice = 1000 };
damagedItem.Tags.Add(new SimpleTagModel {
    DisplayName = "Damaged",
    PriceMultiplier = 0.6f,
    IsRevealedToPlayer = true
});

var value = evaluationService.Evaluate(player, damagedItem);
// Result: 600 (1000 × 0.6)
```

## Service Registration

### Dependency Injection Setup

```csharp
// In ProjectInstaller.InstallBindings()
Container.Bind<IEvaluationService>()
    .To<EvaluationService>()
    .AsSingle();
```

- **Service Registration**: Evaluation service registered in DI container
- **Singleton Pattern**: Single instance for consistent evaluation across game
- **Interface Binding**: Service bound to interface for loose coupling
