# Tag System Documentation

## Overview

The Tag System is a flexible framework for adding metadata to items in the game. Tags can affect item prices, mechanics, and provide additional information to players. The system integrates with skills to determine which tags are visible to different characters.

## Architecture

### Base Classes

- **`BaseTagPrototype`** - Abstract ScriptableObject class that defines tag properties
- **`BaseTagModel`** - Abstract runtime class that holds tag data during gameplay

### Services

- **`ITagRepositoryService`** - Interface for tag data access
- **`TagRepositoryService`** - Implementation that loads and manages tag prototypes

### Tag Types

The system supports three main tag categories:

#### 1. Simple Tags (`SimpleTagPrototype` / `SimpleTagModel`)

- Basic boolean tags without additional data
- Example: "Counterfeit", "Authentic", "Damaged"

#### 2. Text Tags (`TextTagPrototype` / `TextTagModel`)

- Tags that carry string information
- Example: "Autograph" with value "John Doe", "Inscription" with value "Property of..."

#### 3. Numeric Tags (`NumericTagPrototype` / `NumericTagModel`)

- Tags that carry numerical values
- Example: "Wear" with value 75%, "Age" with value 25 years

## Tag Properties

### Core Properties

- **`TagType`** - Category of the tag (Condition, Rarity, Feature, etc.)
- **`DisplayName`** - Human-readable name for the tag
- **`Description`** - Detailed description of what the tag represents
- **`Hidden`** - Whether this tag should be displayed in UI
- **`Color`** - Visual color for the tag

### Specialized Properties

- **`RequiredSkills`** - Array of skill requirements (skill type + required level) to reveal this tag
- **`PriceMultiplier`** - Multiplier that affects item price when this tag is present
- **`AppearanceChance`** - Probability of this tag appearing on an item (0.0 to 1.0)

## Tag Types

Based on the `TagType` enum:

- **`Undefined`** (0) - Default undefined tag
- **`Condition`** (1) - Item condition tags
- **`Rarity`** (2) - Item rarity tags
- **`Feature`** (3) - Special feature tags
- **`Origin`** (4) - Item origin tags
- **`History`** (5) - Historical significance tags
- **`Ownership`** (6) - Ownership-related tags
- **`Documents`** (7) - Documentation tags
- **`LegalStatus`** (8) - Legal status tags
- **`Cultural`** (9) - Cultural significance tags
- **`Age`** (10) - Age-related tags
- **`Manufacturer`** (11) - Manufacturer tags
- **`Authenticity`** (12) - Authenticity verification tags

## Item Condition System

### Condition Through Tags

Instead of using a dedicated condition enum, the system expresses item condition through specific condition tags:

- **Pristine Tag**: Perfect condition with 1.0x price multiplier
- **Used Tag**: Good condition with 0.9x price multiplier
- **Worn Tag**: Normal condition with 0.8x price multiplier
- **Damaged Tag**: Poor condition with 0.6x price multiplier
- **Broken Tag**: Broken condition with 0.3x price multiplier
- **Destroyed Tag**: Destroyed condition with 0.1x price multiplier

### Condition Tag Configuration

Each condition tag is configured as a separate SimpleTagPrototype with:

- **Display Name**: "Pristine", "Used", "Worn", "Damaged", "Broken", "Destroyed"
- **Description**: Detailed explanation of the condition level
- **Tag Type**: Set to Condition
- **Color**: Distinct visual representation (green for pristine, brown for damaged, etc.)
- **Price Multiplier**: Affects final item price based on condition
- **Appearance Chance**: Probability of the condition tag appearing

### Example Condition Tags

- **Pristine**: Price multiplier 1.0x, 15% appearance chance, green color
- **Used**: Price multiplier 0.9x, 25% appearance chance, light green color
- **Worn**: Price multiplier 0.8x, 35% appearance chance, yellow color
- **Damaged**: Price multiplier 0.6x, 18% appearance chance, brown color
- **Broken**: Price multiplier 0.3x, 5% appearance chance, dark brown color
- **Destroyed**: Price multiplier 0.1x, 2% appearance chance, mud brown color

## Item Rarity System

### Rarity Through Specific Tags

The system expresses item rarity through dedicated rarity tags rather than using a rarity enum:

- **Common Tag**: Basic rarity with low price multipliers
- **Uncommon Tag**: Moderate rarity with medium price multipliers
- **Rare Tag**: High rarity with significant price multipliers
- **Very Rare Tag**: Exceptional rarity with high price multipliers
- **Unique Tag**: One-of-a-kind rarity with maximum price multipliers

### Rarity Tag Configuration

Each rarity tag is configured as a separate SimpleTagPrototype with:

- **Display Name**: "Common", "Uncommon", "Rare", etc.
- **Description**: Explanation of what makes items of this rarity special
- **Tag Type**: Usually set to Feature or Condition
- **Color**: Distinct visual representation (green for uncommon, blue for rare, etc.)
- **Price Multiplier**: Affects final item price
- **Appearance Chance**: Probability of the rarity tag appearing

### Example Rarity Tags

- **Common**: Price multiplier 1.0x, high appearance chance
- **Uncommon**: Price multiplier 1.2x, moderate appearance chance
- **Rare**: Price multiplier 1.5x, low appearance chance
- **Very Rare**: Price multiplier 2.0x, very low appearance chance
- **Unique**: Price multiplier 3.0x, extremely low appearance chance

## Skill Integration

### Skill Requirements for Tag Visibility

Tags can require specific skills to be revealed to characters:

```csharp
[System.Serializable]
public struct SkillRequirement
{
    public SkillType SkillType;
    public int RequiredLevel;
}
```

### Player Skills

The player starts with all skills at level 2 by default:

```csharp
// In PlayerService.InitializePlayerSkills()
Player.Skills[skillType].Level = 2; // TODO: Load from config later
```

**Available Skill Types:**

- **NegotiationAmateur** (1) - Basic negotiation skills
- **NegotiationAdvanced** (2) - Advanced negotiation techniques
- **NegotiationExpert** (3) - Expert-level negotiation mastery
- **InspectionAmateur** (4) - Basic item inspection
- **InspectionAdvanced** (5) - Advanced inspection techniques
- **InspectionExpert** (6) - Expert-level inspection mastery
- **RestorationAmateur** (7) - Basic item restoration
- **RestorationAdvanced** (8) - Advanced restoration techniques
- **RestorationExpert** (9) - Expert-level restoration mastery
- **KnowledgeAmateur** (10) - Basic knowledge skills
- **KnowledgeAdvanced** (11) - Advanced knowledge techniques
- **KnowledgeExpert** (12) - Expert-level knowledge mastery

### Customer Skills

Customers have randomly generated skill levels that affect their ability to see tags:

```csharp
// In CustomerFactoryService.GenerateRandomCustomer()
var customer = new Customer();
// Skills are initialized with random levels
```

### Tag Revelation Logic

Tags are revealed based on skill requirements and character type:

```csharp
// In ItemInspectionService.Inspect()
foreach (var skillType in tag.RequiredSkills)
{
    if (character.Skills.TryGetValue(skillType, out var skill))
    {
        int skillLevel = skill.Level;
        float chance = skillLevel * 20f; // 20% per skill level

        var randomValue = UnityEngine.Random.Range(0f, 1f) * 100f;
        if (randomValue <= chance)
        {
            if (isPlayer)
            {
                tag.IsRevealedToPlayer = true;
            }
            else if (isCustomer)
            {
                tag.IsRevealedToCustomer = true;
            }

            revealedTags.Add(tag);
            break;
        }
    }
}
```

**Skill Level Impact:**

- **Level 1**: 20% chance to reveal tag
- **Level 2**: 40% chance to reveal tag
- **Level 3**: 60% chance to reveal tag
- **Level 4**: 80% chance to reveal tag
- **Level 5**: 100% chance to reveal tag

## Item Integration

### ItemPrototype Configuration

```csharp
[Header("Tags")]
public List<BaseTagPrototype> requiredTags;    // Always added to items
public List<TagLimit> allowedTags;             // Added based on probability
```

### TagLimit Structure

```csharp
[System.Serializable]
public class TagLimit
{
    public TagType TagType;
    public int MaxCount;  // Maximum instances of this tag type
}
```

## Tag Loading and Management

### Tag Repository Service

The `TagRepositoryService` manages all tag prototypes and loads them from Resources:

```csharp
public interface ITagRepositoryService
{
    void Load();
    BaseTagPrototype GetTagPrototype(TagType tagType);
    IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes();
}
```

### Loading Process

Tags are automatically loaded during game initialization in `LoadLevelState`:

```csharp
public void Enter()
{
    _itemRepositoryService.Load();
    _skillRepositoryService.Load();
    _tagRepositoryService.Load();  // Tags loaded here
    _sceneLoader.Load("MainScene", () => {
        _stateMachine.Enter<GameLoopState>();
    });
}
```

### Accessing Tag Prototypes

```csharp
// Get a specific tag prototype by type
var conditionTag = tagRepository.GetTagPrototype(TagType.Condition);

// Get all available tag prototypes
var allTags = tagRepository.GetAllTagPrototypes();
```

## Tag Generation

### Automatic Tag Assignment

When an item is created:

1. **Required tags** are always added (ignoring probability)
2. **Allowed tags** are added based on:
   - `AppearanceChance` probability
   - `MaxCount` limit per tag type
   - Current count of existing tags

### Generation Process

```csharp
private void InitializeItemTags(ItemModel item, ItemPrototype prototype)
{
    if (prototype == null) return;

    // Add all required tags first (ignoring probability)
    foreach (var requiredTag in prototype.requiredTags)
    {
        if (requiredTag != null)
        {
            var tagModel = CreateTagModelFromPrototype(requiredTag);
            if (tagModel != null)
            {
                item.Tags.Add(tagModel);
            }
        }
    }

    // Process available tags based on probability and max count
    foreach (var tagLimit in prototype.allowedTags)
    {
        int maxCount = tagLimit.MaxCount;
        int currentCount = item.Tags.Count(t => t.TagType == tagLimit.TagType);
        int remainingSlots = maxCount - currentCount;

        if (remainingSlots <= 0) continue;

        // Try to add tags based on probability
        for (int i = 0; i < remainingSlots; i++)
        {
            if (Random.Range(0f, 1f) <= GetTagPrototype(tagLimit.TagType)?.AppearanceChance)
            {
                var tagModel = CreateTagModelFromPrototype(GetTagPrototype(tagLimit.TagType));
                if (tagModel != null)
                {
                    item.Tags.Add(tagModel);
                }
            }
        }
    }
}
```

### Tag Model Creation

```csharp
private BaseTagModel CreateTagModelFromPrototype(BaseTagPrototype prototype)
{
    if (prototype == null) return null;

    return prototype switch
    {
        SimpleTagPrototype simplePrototype => new SimpleTagModel(simplePrototype),
        TextTagPrototype textPrototype => new TextTagModel(textPrototype),
        NumericTagPrototype numericPrototype => new NumericTagModel(numericPrototype),
        _ => null
    };
}
```

### Tag Model Classes

#### BaseTagModel

```csharp
public abstract class BaseTagModel
{
    public TagType TagType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public SkillRequirement[] RequiredSkills { get; set; }
    public float PriceMultiplier { get; set; }
    public float AppearanceChance { get; set; }
    public bool IsRevealedToPlayer { get; set; }
    public bool IsRevealedToCustomer { get; set; }
    public bool Hidden { get; set; }
    public Color Color { get; set; }
}
```

**Note:** `SkillRequirement` is defined in a separate file `Assets/Scripts/Models/Tags/SkillRequirement.cs` and is used by both `BaseTagModel` and `BaseTagPrototype`.

### SimpleTagModel

```csharp
public class SimpleTagModel : BaseTagModel
{
    public SimpleTagModel(SimpleTagPrototype prototype) : base(prototype)
    {
    }
}
```

### TextTagModel

```csharp
public class TextTagModel : BaseTagModel
{
    public string TextValue { get; set; }

    public TextTagModel(TextTagPrototype prototype) : base(prototype)
    {
        if (prototype != null)
        {
            TextValue = prototype.DefaultTextValue;
        }
    }
}
```

### NumericTagModel

```csharp
public class NumericTagModel : BaseTagModel
{
    public float NumericValue { get; set; }

    public NumericTagModel(NumericTagPrototype prototype) : base(prototype)
    {
        if (prototype != null)
        {
            NumericValue = prototype.DefaultNumericValue;
        }
    }
}
```

## Usage Examples

### Creating Tag Prototypes

#### Simple Tag (Boolean)

```csharp
// Create a "Fake" tag that requires Appraisal skill level 2
var fakeTag = ScriptableObject.CreateInstance<SimpleTagPrototype>();
fakeTag.TagType = TagType.Authenticity;
fakeTag.DisplayName = "Fake";
fakeTag.Description = "Item is a counterfeit or replica";
fakeTag.RequiredSkills = new SkillRequirement[]
{
    new SkillRequirement { SkillType = SkillType.Appraisal, RequiredLevel = 2 }
};
fakeTag.PriceMultiplier = 0.3f;
fakeTag.AppearanceChance = 0.1f;
fakeTag.Hidden = false;
fakeTag.Color = Color.red;
```

#### Text Tag

```csharp
// Create an "Autograph" tag that requires Celebrity Knowledge skill level 3
var autographTag = ScriptableObject.CreateInstance<TextTagPrototype>();
autographTag.TagType = TagType.History;
autographTag.DisplayName = "Autograph";
autographTag.Description = "Signed by a famous person";
autographTag.DefaultTextValue = "Unknown Celebrity";
autographTag.RequiredSkills = new SkillRequirement[]
{
    new SkillRequirement { SkillType = SkillType.CelebrityKnowledge, RequiredLevel = 3 }
};
autographTag.PriceMultiplier = 2.5f;
autographTag.AppearanceChance = 0.05f;
autographTag.Hidden = false;
autographTag.Color = Color.gold;
```

#### Numeric Tag

```csharp
// Create a "Wear" tag that requires Inspection skill level 1
var wearTag = ScriptableObject.CreateInstance<NumericTagPrototype>();
wearTag.TagType = TagType.Condition;
wearTag.DisplayName = "Wear";
wearTag.Description = "Percentage of wear and tear";
wearTag.DefaultNumericValue = 50f;
wearTag.MinValue = 0f;
wearTag.MaxValue = 100f;
wearTag.Unit = "%";
wearTag.RequiredSkills = new SkillRequirement[]
{
    new SkillRequirement { SkillType = SkillType.Inspection, RequiredLevel = 1 }
};
wearTag.PriceMultiplier = 0.8f;
wearTag.AppearanceChance = 0.8f;
wearTag.Hidden = false;
wearTag.Color = Color.orange;
```

### Adding Tags to Items

1. Open ItemPrototype in Inspector
2. Add required tags to `requiredTags` list
3. Configure `allowedTags` with TagType and MaxCount
4. Tags will automatically appear on items when generated

### Runtime Access

```csharp
// Check if item has specific tag
bool hasFeatureTag = item.Tags.Any(t => t.TagType == TagType.Feature);

// Get all condition tags
var conditionTags = item.Tags.Where(t => t.TagType == TagType.Condition);

// Check if tag is revealed to player
bool isRevealedToPlayer = item.Tags.Any(t => t.TagType == TagType.Authenticity && t.IsRevealedToPlayer);

// Check if tag is revealed to customer
bool isRevealedToCustomer = item.Tags.Any(t => t.TagType == TagType.Authenticity && t.IsRevealedToCustomer);

// Calculate price multiplier from tags
float totalMultiplier = item.Tags.Aggregate(1f, (mult, tag) => mult * tag.PriceMultiplier);

// Find rare items by looking for rarity tags by TagType and ClassId
var rareItems = items.Where(item => item.Tags.Any(tag => tag.TagType == TagType.Rarity && tag.ClassId == "rare_tag_class_id"));

// Find items with high price multipliers (likely rare)
var valuableItems = items.Where(item => item.Tags.Any(tag => tag.PriceMultiplier > 2.0f));

// Check item condition through tags using TagType and ClassId
var conditionTag = item.Tags.FirstOrDefault(t => t.TagType == TagType.Condition);
if (conditionTag != null)
{
    // Use ClassId for precise identification instead of DisplayName
    switch (conditionTag.ClassId)
    {
        case "pristine_tag_class_id": // Perfect condition
        case "used_tag_class_id":     // Good condition
        case "worn_tag_class_id":     // Normal condition
        case "damaged_tag_class_id":  // Poor condition
        case "broken_tag_class_id":   // Broken condition
        case "destroyed_tag_class_id": // Destroyed condition
    }
}

// DisplayName now contains localization keys directly (e.g., "tag.condition.destroyed")
// Use ILocalizationService.GetLocalization(tag.DisplayName) to get translated text

// Access tag descriptions for UI display
var tagDescription = item.Tags.FirstOrDefault(t => t.TagType == TagType.Condition)?.Description;
if (!string.IsNullOrEmpty(tagDescription))
{
    Debug.Log($"Condition description: {tagDescription}");
}
```

## Integration with Other Systems

### Evaluation System

Tags affect item evaluation through price multipliers:

```csharp
// In EvaluationService.Evaluate()
foreach (var tag in item.Tags)
{
    bool isTagRevealed = isPlayer ? tag.IsRevealedToPlayer : tag.IsRevealedToCustomer;

    if (isTagRevealed)
    {
        finalPrice = (long)(finalPrice * tag.PriceMultiplier);
    }
}
```

### Inspection System

Tags are revealed through the inspection process:

```csharp
// In ItemInspectionService.Inspect()
var revealedTags = new List<BaseTagModel>();
// Tags are revealed based on character skills
return revealedTags; // Only newly revealed tags
```

### Negotiation System

Revealed tags affect negotiation outcomes:

```csharp
// In NegotiationService
public event Action<List<BaseTagModel>> OnTagsRevealed;
// Tags revealed during inspection trigger UI updates
```
