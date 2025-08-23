# DayTimeController Documentation

## Overview

DayTimeController is a Unity component that manages scene lighting based on in-game time. It automatically adjusts the brightness of sprite objects and rotates the sky sprite to simulate day/night cycles.

## Features

- **Automatic Sky Rotation**: Rotates the sky sprite 360 degrees over 24 in-game hours
- **Dynamic Lighting**: Adjusts brightness of all child sprites based on time of day
- **Configurable Objects**: Each lighting object can have custom brightness coefficients
- **Real-time Updates**: Responds to time changes from the TimeService
- **Runtime Management**: Add/remove lighting objects during gameplay

## Setup

### 1. Scene Setup

1. Create an empty GameObject in your scene
2. Add the `DayTimeController` component to it
3. Assign a sky object (Transform) to the `Sky` field
4. Add lighting objects to the `Lighting Objects` list

### 2. Sky Object

The sky object should be a Transform containing a sprite that represents the sky/background. This will be rotated automatically to show time progression.

### 3. Lighting Objects

Each lighting object in the list contains:

- **GameObject**: The target object whose sprites will be affected
- **Lighting Coefficient**: How much this object responds to lighting changes (0.0 - 1.0)
- **Min Brightness**: Minimum brightness level (0.0 - 1.0)
- **Max Brightness**: Maximum brightness level (0.0 - 1.0)

## Configuration

### Inspector Fields

```csharp
[Header("Sky Settings")]
[SerializeField] private Transform _sky;                    // Sky object to rotate

[Header("Lighting Objects")]
[SerializeField] private List<LightingObject> _lightingObjects;  // List of objects to light

[Header("Time Settings")]
[SerializeField] private float _skyRotationSpeed = 15f;     // Rotation speed in degrees per hour
```

### Lighting Object Structure

```csharp
[System.Serializable]
public class LightingObject
{
    public GameObject gameObject;           // Target GameObject
    [Range(0f, 1f)] public float lightingCoefficient = 1f;  // Lighting sensitivity
    [Range(0f, 1f)] public float minBrightness = 0.1f;      // Darkest possible brightness
    [Range(0f, 1f)] public float maxBrightness = 1f;        // Brightest possible brightness
}
```

## How It Works

### Time Calculation

The controller converts in-game time to a decimal value (0.0 - 1.0):

- **0.0**: Midnight
- **0.25**: 6:00 AM (dawn)
- **0.5**: 12:00 PM (noon)
- **0.75**: 6:00 PM (dusk)
- **1.0**: Midnight

### Lighting Curve

The lighting follows a bell curve pattern:

- **Peak brightness**: Around noon (12:00 PM)
- **Gradual dimming**: Evening and night
- **Gradual brightening**: Morning and dawn

### Sky Rotation

The sky rotates smoothly:

- **0°**: Midnight
- **90°**: 6:00 AM
- **180°**: 12:00 PM (noon)
- **270°**: 6:00 PM
- **360°**: Midnight

## Usage Examples

### Basic Setup

```csharp
// The controller automatically works once set up in the inspector
// No additional code needed for basic functionality
```

### Runtime Object Management

```csharp
[Inject] private DayTimeController _dayTimeController;

// Add a new lighting object at runtime
public void AddNewLightingObject(GameObject obj)
{
    _dayTimeController.AddLightingObject(obj, 0.8f, 0.2f, 1.0f);
}

// Remove a lighting object
public void RemoveLightingObject(GameObject obj)
{
    _dayTimeController.RemoveLightingObject(obj);
}
```

### Custom Lighting Coefficients

- **1.0**: Full lighting response (default)
- **0.5**: Half lighting response (less dramatic changes)
- **0.0**: No lighting response (constant brightness)
- **2.0**: Double lighting response (more dramatic changes)

## Dependencies

- **ITimeService**: Required for time updates
- **Zenject**: Used for dependency injection
- **Unity Engine**: Core Unity functionality

## Performance Notes

- The controller only updates when time changes (not every frame)
- Sprite color modifications are efficient
- Sky rotation uses smooth interpolation for visual quality
- All lighting objects are cached on initialization

## Troubleshooting

### Common Issues

1. **No lighting changes**: Check if TimeService is properly injected
2. **Sky not rotating**: Verify the Sky field is assigned in inspector
3. **Sprites not changing**: Ensure objects have SpriteRenderer components
4. **Performance issues**: Limit the number of lighting objects

### Debug Information

The controller logs errors to the console for:

- Missing TimeService injection
- Null GameObject references
- Missing SpriteRenderer components

## Integration with Existing Systems

The DayTimeController integrates seamlessly with:

- **TimeService**: Receives time updates automatically
- **Zenject DI**: Automatically injected with required services
- **Unity UI**: Works with any GameObject containing sprites
- **Scene Management**: Persists across scene loads if properly configured
