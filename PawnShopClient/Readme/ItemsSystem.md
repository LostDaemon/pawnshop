# Items System Documentation

## Overview

The Items System is the core framework for managing game objects that players can buy, sell, inspect, and interact with. It provides a flexible structure for creating diverse item types with customizable properties, tags, and behaviors.

## Architecture

### Core Classes

- **`ItemPrototype`** - ScriptableObject that defines item properties and behavior
- **`ItemModel`** - Runtime class that holds item data during gameplay
- **`IItemRepositoryService`** - Interface for item data access and management
- **`ItemRepositoryService`** - Implementation of item repository with tag generation

### Data Flow

```
ItemPrototype (ScriptableObject) → ItemModel (Runtime) → Game Logic
     ↓                              ↓
  Static Data                  Dynamic Data
  - Base Properties            - Runtime State
  - Tag Configuration          - Generated Tags
  - Visual Assets              - Current Values
```

## Item Properties

### Base Properties (ItemPrototype)

- **`ClassId`** - Unique identifier for item type
- **`Name`** - Human-readable item name
- **`Description`** - Detailed item description
- **`ImageId`** - Reference to item sprite/image
- **`BasePrice`** - Base market value of the item
- **`Scale`** - Visual scale factor for rendering

### Runtime Properties (ItemModel)

- **`Id`** - Unique instance identifier (GUID)
- **`ClassId`** - Reference to prototype
- **`PurchasePrice`** - Price paid when buying
- **`SellPrice`** - Price received when selling
- **`IsFake`** - Whether item is counterfeit
- **`Condition`** - Current item condition (0-100)
- **`Inspected`** - Whether item has been examined
- **`Tags`** - List of applied tags affecting properties

## Tag Integration

### Tag Configuration

Items can have two types of tags:

```csharp
[Header("Tags")]
public List<BaseTagPrototype> requiredTags;    // Always present
public List<TagLimit> allowedTags;             // Probability-based
```

### Tag Generation Process

1. **Required tags** are always added during item creation
2. **Allowed tags** are added based on:
   - Individual tag probability (`AppearanceChance`)
   - Maximum count limits (`MaxCount`)
   - Current tag count validation

### Tag Effects

- **Price modification** through `PriceMultiplier`
- **Mechanical effects** (hidden tags)
- **Visual representation** with colors
- **Skill requirements** for tag revelation

## Item Condition System

### Condition Through Tags

Instead of using a dedicated condition enum, the system expresses item condition through specific condition tags:

- **Pristine Items**: Items with "Pristine" condition tag (1.0x price multiplier)
- **Used Items**: Items with "Used" condition tag (0.9x price multiplier)
- **Worn Items**: Items with "Worn" condition tag (0.8x price multiplier)
- **Damaged Items**: Items with "Damaged" condition tag (0.6x price multiplier)
- **Broken Items**: Items with "Broken" condition tag (0.3x price multiplier)
- **Destroyed Items**: Items with "Destroyed" condition tag (0.1x price multiplier)

### Condition Tag Properties

Each condition tag affects items differently:

- **Price Multipliers**: Better condition = higher price multipliers
- **Appearance Chances**: Better condition = lower appearance probability
- **Visual Colors**: Each condition has distinct color representation
- **Skill Requirements**: Some condition tags require specific skills to reveal

## Item Rarity System

### Rarity Through Tags

Instead of using a dedicated rarity enum, the system expresses item rarity through specific rarity tags:

- **Common Items**: Items with "Common" rarity tag
- **Uncommon Items**: Items with "Uncommon" rarity tag
- **Rare Items**: Items with "Rare" rarity tag
- **Very Rare Items**: Items with "Very Rare" rarity tag
- **Unique Items**: Items with "Unique" rarity tag

### Rarity Tag Properties

Each rarity tag affects items differently:

- **Price Multipliers**: Higher rarity = higher price multipliers
- **Appearance Chances**: Higher rarity = lower appearance probability
- **Visual Colors**: Each rarity has distinct color representation
- **Skill Requirements**: Some rarity tags require specific skills to reveal

## Item Creation

### Automatic Generation

```csharp
// In ItemRepositoryService.GetItem()
var result = new ItemModel()
{
    Id = System.Guid.NewGuid().ToString(),
    ClassId = itemPrototype.ClassId,
    Name = itemPrototype.Name,
    ImageId = itemPrototype.ImageId,
    BasePrice = itemPrototype.BasePrice,
    IsFake = _random.NextDouble() < 0.25,
    Scale = itemPrototype.Scale,
    Description = itemPrototype.Description,
    PurchasePrice = 0,
    SellPrice = 0,
    Inspected = false,
    Condition = _random.Next(0, 100)
};

// Initialize tags for the item
InitializeItemTags(result, itemPrototype);

return result;
```

### Random Item Generation

```csharp
public ItemModel GetRandomItem()
{
    int index = _random.Next(_items.Count);
    var itemPrototype = _items[index];
    return GetItem(itemPrototype.ClassId);
}
```

### Specific Item Retrieval

```csharp
public ItemModel GetItem(string classId)
{
    var itemPrototype = _items.FirstOrDefault(item => item.ClassId == classId);
    if (itemPrototype == null) return null;

    // Create item with random properties
    var result = new ItemModel()
    {
        // ... property initialization
    };

    InitializeItemTags(result, itemPrototype);
    return result;
}
```

## Item States

### Condition System

Items have a condition value from 0-100, but their actual condition is determined by tags:

- **0-20**: Broken/Destroyed (determined by condition tags)
- **21-40**: Poor condition (determined by condition tags)
- **41-60**: Normal condition (determined by condition tags)
- **61-80**: Good condition (determined by condition tags)
- **81-100**: Perfect condition (determined by condition tags)

### Fake Detection

- 25% chance for items to be counterfeit
- Affects item value and player trust
- Requires inspection skills to detect

### Inspection System

- Items start as uninspected
- Inspection reveals hidden properties
- Required for accurate pricing

## Repository Management

### Service Interface

```csharp
public interface IItemRepositoryService
{
    ItemModel GetRandomItem();
    ItemModel GetItem(string classId);
    void Load();
    void AddItem(ItemPrototype itemPrototype);
    void RemoveItem(ItemPrototype itemPrototype);
}
```

### Data Loading

```csharp
public void Load()
{
    _items.Clear();
    _items.AddRange(Resources.LoadAll<ItemPrototype>(@"ScriptableObjects\Items").ToList());
    Debug.Log($"Loaded {_items.Count} item prototypes.");
}
```

### Item Management

```csharp
public void AddItem(ItemPrototype itemPrototype)
{
    if (itemPrototype != null && !_items.Contains(itemPrototype))
    {
        _items.Add(itemPrototype);
    }
}

public void RemoveItem(ItemPrototype itemPrototype)
{
    if (itemPrototype != null)
    {
        _items.Remove(itemPrototype);
    }
}
```

## Item Categories

### Basic Items

- **Electronics** - Laptops, phones, tablets
- **Jewelry** - Rings, necklaces, watches
- **Antiques** - Furniture, artwork, collectibles
- **Tools** - Hand tools, power tools, equipment
- **Clothing** - Designer items, vintage pieces

### Special Properties

- **Condition-based pricing** - Better condition = higher value (through tags)
- **Fake detection** - 25% chance of counterfeit items
- **Tag integration** - Multiple tags affect item properties
- **Dynamic pricing** - Prices change based on market conditions

## Price Calculation

### Base Pricing

- **Base Price** - Starting value from ItemPrototype
- **Condition Modifier** - Condition affects final price (through condition tags)
- **Tag Multipliers** - Tags can increase/decrease value
- **Market Factors** - Supply and demand considerations

### Price Factors

```csharp
// Example: Calculate final price with tags
public float CalculateFinalPrice(ItemModel item)
{
    float basePrice = item.BasePrice;

    // Apply condition modifier through tags
    var conditionTag = item.Tags.FirstOrDefault(t => t.TagType == TagType.Condition);
    float conditionMultiplier = conditionTag?.PriceMultiplier ?? 1.0f;

    // Apply other tag multipliers
    float otherTagMultiplier = item.Tags
        .Where(t => t.TagType != TagType.Condition)
        .Aggregate(1f, (mult, tag) => mult * tag.PriceMultiplier);

    return basePrice * conditionMultiplier * otherTagMultiplier;
}
```

## Integration Points

### With Wallet System

```csharp
// In NegotiationService.TryPurchase()
var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
if (success)
{
    CurrentItem.PurchasePrice = offeredPrice;
    _inventory.Put(CurrentItem);
}
```

### With Storage System

```csharp
// In SellService.ScheduleForSale()
if (!_inventory.Withdraw(item))
    return false;

_sellStorage.Put(item);
```

### With Tag System

```csharp
// Tags are automatically generated when items are created
InitializeItemTags(result, itemPrototype);
```

## Usage Examples

### Creating Item Prototypes

1. Right-click in Project window
2. Create → ScriptableObjects → ItemPrototype
3. Configure properties:
   - Set unique `ClassId`
   - Enter `Name` and `Description`
   - Set `BasePrice` and `Scale`
   - Configure `ImageId` for sprite reference
   - Add required and allowed tags

### Runtime Item Access

```csharp
// Get random item for testing
var randomItem = itemRepository.GetRandomItem();

// Get specific item type
var laptop = itemRepository.GetItem("laptop_001");

// Check item properties
bool isFake = item.IsFake;
int condition = item.Condition;
long basePrice = item.BasePrice;

// Access item tags
var conditionTags = item.Tags.Where(t => t.TagType == TagType.Condition);

// Check item condition through tags
var conditionTag = item.Tags.FirstOrDefault(t => t.TagType == TagType.Condition);
if (conditionTag != null)
{
    switch (conditionTag.DisplayName)
    {
        case "Pristine": // Perfect condition
        case "Used":     // Good condition
        case "Worn":     // Normal condition
        case "Damaged":  // Poor condition
        case "Broken":   // Broken condition
        case "Destroyed": // Destroyed condition
    }
}

// Check item rarity through tags
bool isRare = item.Tags.Any(t => t.DisplayName == "Rare");
bool isUnique = item.Tags.Any(t => t.DisplayName == "Unique");
```

### Item Processing

```csharp
// Example: Process item for sale
public void ProcessItemForSale(ItemModel item)
{
    // Check condition through tags
    var conditionTag = item.Tags.FirstOrDefault(t => t.TagType == TagType.Condition);
    if (conditionTag != null)
    {
        switch (conditionTag.DisplayName)
        {
            case "Pristine":
                Debug.Log($"Item {item.Name} is in perfect condition");
                break;
            case "Used":
                Debug.Log($"Item {item.Name} is in good condition");
                break;
            case "Worn":
                Debug.Log($"Item {item.Name} is in normal condition");
                break;
            case "Damaged":
                Debug.Log($"Item {item.Name} is damaged");
                break;
            case "Broken":
                Debug.Log($"Item {item.Name} is broken");
                break;
            case "Destroyed":
                Debug.Log($"Item {item.Name} is destroyed");
                break;
        }
    }

    if (item.IsFake)
    {
        // Handle counterfeit item
        Debug.Log($"Item {item.Name} is counterfeit");
    }

    // Check rarity for pricing strategy
    var rarityTag = item.Tags.FirstOrDefault(t =>
        t.DisplayName == "Rare" || t.DisplayName == "Unique");
    if (rarityTag != null)
    {
        Debug.Log($"Item {item.Name} is {rarityTag.DisplayName} - consider premium pricing");
    }

    // Add to sell storage
    _sellService.ScheduleForSale(item);
}
```
