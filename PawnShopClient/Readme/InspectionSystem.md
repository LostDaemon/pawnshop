# Inspection System Documentation

## Overview

The Inspection System is responsible for revealing hidden item properties and tags based on character skills.

## Architecture

### Core Components

- **`IItemInspectionService`** - Interface defining inspection operations
- **`ItemInspectionService`** - Main implementation of item inspection logic

### Data Flow

```
Character + Item → Inspection Service → Skill Check → Tag Revelation → Inspection Result
```

## Core Functionality

### Item Inspection

The main inspection method reveals hidden item properties:

```csharp
public interface IItemInspectionService
{
    public List<BaseTagModel> Inspect(ICharacter character, ItemModel item);
}
```

### Inspection Process

The inspection follows a systematic approach to reveal hidden tags:

```csharp
public List<BaseTagModel> Inspect(ICharacter character, ItemModel item)
{
    var revealedTags = new List<BaseTagModel>();

    if (item?.Tags == null)
    {
        return revealedTags;
    }

    bool isPlayer = character is Player;
    bool isCustomer = character is Customer;

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
            // Tag already revealed, skip
            continue;
        }

        if (tag.RequiredSkills == null || tag.RequiredSkills.Length == 0)
        {
            continue;
        }

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
    }

    return revealedTags; // Only newly revealed tags
}
```

## Inspection Logic

### Tag Visibility States

Tags have two visibility states for different character types:

```csharp
public abstract class BaseTagModel
{
    public bool IsRevealedToPlayer { get; set; }    // Visible to player
    public bool IsRevealedToCustomer { get; set; }  // Visible to customer
}
```

**Visibility Rules:**

- **Hidden Tags**: Tags that haven't been revealed to a character
- **Revealed Tags**: Tags that are visible and affect item evaluation
- **Character-Specific**: Tags can be revealed to one character but hidden from another

### Skill-Based Revelation

Tags are revealed based on character skills and random chance:

```csharp
// Skill check for tag revelation
int skillLevel = skill.Level;
float chance = skillLevel * 20f; // 20% per skill level

var randomValue = UnityEngine.Random.Range(0f, 1f) * 100f;
if (randomValue <= chance)
{
    // Tag is successfully revealed
    tag.IsRevealedToPlayer = true; // or IsRevealedToCustomer
}
```

**Skill Level Effectiveness:**

- **Level 1**: 20% chance to reveal hidden tags
- **Level 2**: 40% chance to reveal hidden tags
- **Level 3**: 60% chance to reveal hidden tags
- **Level 4**: 80% chance to reveal hidden tags
- **Level 5**: 100% chance to reveal hidden tags

### Required Skills System

Tags can require specific skills to be revealed:

```csharp
[System.Serializable]
public struct SkillRequirement
{
    public SkillType SkillType;
    public int RequiredLevel;
}
```

## Character-Specific Inspection

### Player Inspection

```csharp
bool isPlayer = character is Player;
if (isPlayer)
{
    isTagRevealed = tag.IsRevealedToPlayer;
}
```

### Customer Inspection

```csharp
bool isCustomer = character is Customer;
if (isCustomer)
{
    isTagRevealed = tag.IsRevealedToCustomer;
}
```

## Integration Points

### With Tag System

Inspection directly modifies tag visibility states:

```csharp
// Tags are updated during inspection
if (isPlayer)
{
    tag.IsRevealedToPlayer = true;
}
else if (isCustomer)
{
    tag.IsRevealedToCustomer = true;
}
```

**Tag Properties Modified:**

- **IsRevealedToPlayer**: Player visibility state
- **IsRevealedToCustomer**: Customer visibility state
- **RequiredSkills**: Skills needed for revelation

### With Skill System

Skills determine inspection success rates:

```csharp
// Skill levels affect revelation chances
int skillLevel = skill.Level;
float chance = skillLevel * 20f; // 20% per skill level
```

## Usage Examples

### Basic Item Inspection

```csharp
// Inspect an item as a player
var revealedTags = inspectionService.Inspect(player, item);

// Inspect the same item as a customer
var customerRevealedTags = inspectionService.Inspect(customer, item);
```

### Skill-Dependent Inspection

```csharp
// Player with high inspection skills
var expertPlayer = new Player();
expertPlayer.Skills[SkillType.InspectionExpert] = new Skill { Level = 5 };

// Item with hidden damage requiring inspection skill level 3
var item = new ItemModel();
item.Tags.Add(new SimpleTagModel
{
    DisplayName = "Hidden Damage",
    RequiredSkills = new SkillRequirement[]
    {
        new SkillRequirement { SkillType = SkillType.InspectionExpert, RequiredLevel = 3 }
    }
});

var revealedTags = inspectionService.Inspect(expertPlayer, item);
// Result: Hidden damage tag is revealed (100% chance with level 5 skill)
```

## Service Registration

### Dependency Injection Setup

```csharp
// In ProjectInstaller.InstallBindings()
Container.Bind<IItemInspectionService>()
    .To<ItemInspectionService>()
    .AsSingle();
```

- **Service Registration**: Inspection service registered in DI container
- **Singleton Pattern**: Single instance for consistent inspection across game
- **Interface Binding**: Service bound to interface for loose coupling
