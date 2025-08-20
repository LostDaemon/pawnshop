# Skill System Documentation

## Overview

The Skill System manages player abilities and their progression through learning and practice. It provides a framework for skill-based gameplay mechanics and character development.

## Architecture

### Core Classes

- **`ISkillService`** - Interface defining skill service operations
- **`SkillService`** - Main implementation of skill management
- **`ISkillRepositoryService`** - Interface for skill data access
- **`SkillRepositoryService`** - Implementation of skill repository
- **`SkillPrototype`** - ScriptableObject defining skill properties
- **`Skill`** - Runtime class holding skill data
- **`SkillController`** - UI controller for skill interactions

### Data Flow

```
SkillPrototype → SkillRepository → SkillService → SkillController → UI
     ↓              ↓                ↓              ↓           ↓
  Static Data   Data Access    Skill Logic    User Input   Display
```

## Skill Properties

### Base Properties

- **`SkillType`** - Enum defining the skill category
- **`DisplayName`** - Human-readable name for the skill
- **`Description`** - Detailed description of what the skill does
- **`Glyph`** - FontAwesome hex code for visual representation
- **`MaxLevel`** - Maximum level the skill can reach (default: 5)

### Runtime Properties

- **`Level`** - Current skill level (0 = not learned, 1+ = learned)
- **`RequiredSkills`** - List of skill requirements with required levels
- **`IsLearned`** - Computed property: `Level > 0`

### Skill Requirements Structure

Skills can have dependencies on other skills with specific level requirements:

```csharp
[System.Serializable]
public struct SkillRequirement
{
    public SkillType SkillType;
    public int RequiredLevel;
}
```

Example: A skill might require "Appraisal level 2" and "History level 1" to be learnable.

## Skill Types

Based on the `SkillType` enum:

- **`Undefined`** (0) - Default undefined skill
- **`NegotiationAmateur`** (1) - Basic negotiation skills
- **`NegotiationAdvanced`** (2) - Advanced negotiation techniques
- **`NegotiationExpert`** (3) - Expert-level negotiation mastery
- **`InspectionAmateur`** (4) - Basic item inspection
- **`InspectionAdvanced`** (5) - Advanced inspection techniques
- **`InspectionExpert`** (6) - Expert-level inspection mastery
- **`RestorationAmateur`** (7) - Basic item restoration
- **`RestorationAdvanced`** (8) - Advanced restoration techniques
- **`RestorationExpert`** (9) - Expert-level restoration mastery
- **`KnowledgeAmateur`** (10) - Basic knowledge skills
- **`KnowledgeAdvanced`** (11) - Advanced knowledge techniques
- **`KnowledgeExpert`** (12) - Expert-level knowledge mastery

## Character Skills

### Player Skills

The player starts with all skills at level 2 by default:

```csharp
// In PlayerService.InitializePlayerSkills()
Player.Skills[skillType].Level = 2; // TODO: Load from config later
```

**Player Skill Characteristics:**

- **Starting Level**: All skills begin at level 2
- **Skill Learning**: Skills can be learned through gameplay progression
- **Skill Reset**: All skills can be reset to unlearned state
- **Dependency Management**: Skills respect prerequisite requirements

### Customer Skills

Customers have randomly generated skill levels that affect their behavior:

```csharp
// In CustomerFactoryService.GenerateRandomCustomer()
var customer = new Customer();
// Skills are initialized with random levels
```

**Customer Skill Characteristics:**

- **Random Generation**: Skill levels are randomly assigned
- **Tag Visibility**: Skills determine which item tags customers can see
- **Negotiation Behavior**: Skills affect customer negotiation strategies
- **Item Knowledge**: Skills influence customer understanding of item value

### Skill Impact on Gameplay

#### Tag Visibility

Skills determine which item tags are visible to characters:

```csharp
// In ItemInspectionService.Inspect()
float chance = skillLevel * 20f; // 20% per skill level
var randomValue = UnityEngine.Random.Range(0f, 1f) * 100f;

if (randomValue <= chance)
{
    // Tag is revealed based on skill level
    tag.IsRevealedToPlayer = true; // or IsRevealedToCustomer
}
```

**Skill Level Effectiveness:**

- **Level 1**: 20% chance to reveal hidden tags
- **Level 2**: 40% chance to reveal hidden tags
- **Level 3**: 60% chance to reveal hidden tags
- **Level 4**: 80% chance to reveal hidden tags
- **Level 5**: 100% chance to reveal hidden tags

## Dependency System

### Skill Requirements

Skills can have prerequisites that must be learned first:

```csharp
public bool CanLearnSkill(SkillType skill)
{
    var prototype = _skillRepository.GetSkill(skill);
    if (prototype == null) return false;

    foreach (var requiredSkill in prototype.RequiredSkills)
    {
        if (!IsSkillLearned(requiredSkill))
            return false;
    }

    return true;
}
```

- **Prerequisite Checking**: Skills require specific skills to be learned first
- **Dependency Validation**: Cannot learn skills without meeting requirements
- **Hierarchical Structure**: Skills form a learning tree

### Example Skill Dependencies

```csharp
// Advanced skills require basic skills first
NegotiationAdvanced requires NegotiationAmateur level 2
InspectionExpert requires InspectionAmateur level 3
KnowledgeExpert requires KnowledgeAmateur level 4
```

## Event System

### Skill Learning Events

```csharp
public event Action<SkillType> OnSkillLearned;
```

- **Event Triggering**: Fired when a skill is successfully learned
- **UI Updates**: Controllers subscribe to update availability
- **Dependency Updates**: Other skills may become available

### Skill Management

```csharp
public bool LearnSkill(SkillType skill)
{
    if (skill == SkillType.Undefined)
        return false;

    if (IsSkillLearned(skill))
        return false;

    if (!CanLearnSkill(skill))
        return false;

    _skills[skill].IsLearned = true;
    OnSkillLearned?.Invoke(skill);

    return true;
}
```

- **Validation**: Checks if skill can be learned
- **State Update**: Marks skill as learned
- **Event Firing**: Notifies subscribers of skill acquisition

### Skill Reset

```csharp
public void ResetAllSkills()
{
    foreach (var skill in _skills.Values)
    {
        skill.IsLearned = false;
    }
}
```

- **Complete Reset**: All skills return to unlearned state
- **Fresh Start**: Player can rebuild skill tree from beginning

## UI Integration

### Skill Controller

```csharp
public class SkillController : MonoBehaviour
{
    [SerializeField] private Button _skillButton;
    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private TMP_Text _skillDescriptionText;
    [SerializeField] private Image _skillIcon;

    [Inject] private ISkillService _skillService;
    [Inject] private ISkillRepositoryService _skillRepository;

    private SkillType _skillType;
    private SkillPrototype _prototype;
}
```

- **Button Integration**: Skill button directly on the skill controller
- **Visual Elements**: Name, description, and icon display
- **Service Injection**: Uses dependency injection for services

### Skill Availability

```csharp
private void UpdateSkillAvailability()
{
    bool canLearn = _skillService.CanLearnSkill(_skillType);
    bool isLearned = _skillService.IsSkillLearned(_skillType);

    _skillButton.interactable = canLearn && !isLearned;
    _skillButton.onClick.RemoveAllListeners();

    if (canLearn && !isLearned)
    {
        _skillButton.onClick.AddListener(OnSkillButtonClicked);
    }
}
```

- **Dynamic Updates**: Button availability changes based on skill state
- **Click Handling**: Button only responds when skill can be learned
- **Visual Feedback**: Button interactability reflects current state

### Skill Learning

```csharp
private void OnSkillButtonClicked()
{
    if (_skillService.LearnSkill(_skillType))
    {
        UpdateSkillAvailability();
    }
}
```

- **Direct Learning**: Button click directly learns the skill
- **State Update**: UI refreshes after successful learning
- **Service Integration**: Uses SkillService for learning logic

## Service Management

### Repository Service

```csharp
public interface ISkillRepositoryService
{
    SkillPrototype GetSkill(SkillType skillType);
    void Load();
}
```

- **Data Access**: Provides access to skill prototypes
- **Loading**: Loads skill data from resources
- **Prototype Retrieval**: Gets specific skill by type

### Skill Service

```csharp
public interface ISkillService
{
    bool IsSkillLearned(SkillType skill);
    bool CanLearnSkill(SkillType skill);
    bool LearnSkill(SkillType skill);
    void ResetAllSkills();
    event Action<SkillType> OnSkillLearned;
}
```

- **Skill State**: Manages learned/unlearned status
- **Learning Logic**: Handles skill acquisition
- **Dependency Management**: Validates skill requirements
- **Event System**: Notifies of skill changes

## Installation and Setup

### Project Installer

```csharp
// In ProjectInstaller.InstallBindings()
Container.Bind<ISkillRepositoryService>()
    .To<SkillRepositoryService>()
    .AsSingle();

Container.Bind<ISkillService>()
    .To<SkillService>()
    .AsSingle();
```

- **Service Registration**: Skills registered in dependency injection
- **Singleton Pattern**: Single instance of each service
- **Interface Binding**: Services bound to their interfaces

### Creating Skill Prototypes

1. **Create ScriptableObject**

   - Right-click in Project window
   - Create → PawnShop → Skill
   - Name: "skill_appraisal_advanced"

2. **Configure Basic Information**

   - Display Name: "Advanced Appraisal"
   - Description: "Expert-level item evaluation and authentication"
   - Max Level: 5
   - Glyph: "F06E" (FontAwesome hex code)

3. **Set Skill Type**

   - Skill Type: Appraisal

4. **Configure Requirements**
   - Required Skills: Add SkillRequirement entries
   - Example: Appraisal level 2, History level 1
   - This means the skill requires Appraisal to be at level 2+ and History at level 1+

## Integration Points

### With Tag System

- **Skill Requirements**: Tags can require specific skills to reveal
- **Inspection Skills**: Required for revealing hidden item properties
- **Knowledge Skills**: Required for understanding item details

### With Item System

- **Condition Assessment**: Skills affect ability to evaluate items
- **Fake Detection**: Skills improve counterfeit identification
- **Value Estimation**: Skills enhance price assessment accuracy

### With Evaluation System

- **Price Accuracy**: Higher skill levels provide more accurate evaluations
- **Tag Recognition**: Skills unlock access to hidden item properties
- **Value Calculation**: Skills influence final price calculations

### With Inspection System

- **Tag Revelation**: Skills determine which tags become visible
- **Inspection Success**: Higher skill levels increase inspection effectiveness
- **Hidden Properties**: Skills unlock access to concealed item features

### With Negotiation System

- **Customer Behavior**: Customer skills affect their negotiation strategies
- **Price Expectations**: Skills influence customer price understanding
- **Deal Success**: Skills affect negotiation outcome probabilities
