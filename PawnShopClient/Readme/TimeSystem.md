# Time System Documentation

## Overview

The Time System manages in-game time progression and provides a task scheduler for executing actions at specific times.

## Architecture

### Core Components

- **`ITimeService`** - Interface defining time service operations
- **`TimeService`** - Main implementation of time management and scheduling
- **`GameTime`** - Data structure representing in-game time
- **`TimeDriverController`** - Unity component that drives time progression
- **`TimeLabelController`** - UI controller for displaying current time

### Data Flow

```
Unity Update → TimeDriverController → TimeService.Tick() → Time Advancement → Event Triggering
     ↓              ↓                    ↓                    ↓                ↓
  DeltaTime    Time Service         Time Logic         GameTime Update   Scheduled Events
```

## Time Structure

### GameTime Data Model

```csharp
public struct GameTime
{
    public int Day;      // Current game day (starts from 1)
    public int Hour;     // Hour of day (0-23)
    public int Minute;   // Minute of hour (0-59)
}
```

### Time Representation

- **Format**: "Day X HH:MM" (e.g., "Day 1 08:00")
- **Starting Time**: Day 1, 08:00 (8:00 AM)
- **24-Hour Format**: Uses 24-hour clock system
- **Day Progression**: Days increment automatically at midnight

## Time Progression

### Time Multiplier

```csharp
public float TimeMultiplier { get; set; } = 60f; // 1 real second = 1 in-game minute
```

- **Default Value**: 60x (1 real second = 1 in-game minute)
- **Configurable**: Can be adjusted during runtime

### Time Advancement Logic

```csharp
public void Tick(float deltaTime)
{
    _timeAccumulator += deltaTime * TimeMultiplier;

    while (_timeAccumulator >= 60f)
    {
        AdvanceMinute();
        _timeAccumulator -= 60f;
    }
}
```

- **Accumulation**: Real time accumulates and converts to in-game minutes
- **Minute-by-Minute**: Time advances one minute at a time
- **Event Triggering**: Each minute advancement triggers scheduled events

### Time Calculation

```csharp
private GameTime AddMinutes(GameTime time, int minutesToAdd)
{
    int totalMinutes = time.Hour * 60 + time.Minute + minutesToAdd;
    int newDay = time.Day + totalMinutes / MinutesInDay;
    totalMinutes %= MinutesInDay;

    return new GameTime(newDay, totalMinutes / 60, totalMinutes % 60);
}
```

- **Day Overflow**: Automatically handles day transitions
- **Hour Overflow**: Automatically handles hour transitions

## Task Scheduler

### Scheduling Interface

```csharp
public void Schedule(GameTime time, Action callback)
{
    _scheduledEvents.Add(new ScheduledEvent { Time = time, Callback = callback });
}
```

- **Time-Based**: Schedule actions for specific in-game times
- **Callback System**: Execute arbitrary code when time is reached
- **Automatic Cleanup**: Events are removed after execution

### Scheduled Event Structure

```csharp
private class ScheduledEvent
{
    public GameTime Time;     // When to execute the event
    public Action Callback;   // What to execute
}
```

### Event Triggering

```csharp
private void TriggerEvents(GameTime now)
{
    for (int i = _scheduledEvents.Count - 1; i >= 0; i--)
    {
        if (IsSameOrPast(_scheduledEvents[i].Time, now))
        {
            _scheduledEvents[i].Callback?.Invoke();
            _scheduledEvents.RemoveAt(i);
        }
    }
}
```

- **Automatic Execution**: Events trigger when their time is reached
- **Reverse Iteration**: Safe removal during iteration
- **Immediate Cleanup**: Events are removed after execution

### Time Comparison Logic

```csharp
private bool IsSameOrPast(GameTime a, GameTime b)
{
    return a.Day < b.Day ||
           (a.Day == b.Day && a.Hour < b.Hour) ||
           (a.Day == b.Day && a.Hour == b.Hour && a.Minute <= b.Minute);
}
```

- **Day Priority**: Earlier days always come first
- **Hour Priority**: Earlier hours within the same day
- **Minute Priority**: Earlier or same minutes within the same hour

## Integration Points

### With Sell Service

```csharp
// In SellService.ScheduleForSale()
int delayHours = UnityEngine.Random.Range(1, 4);
var scheduleTime = AddHours(_timeService.CurrentTime, delayHours);
_timeService.Schedule(scheduleTime, () => SellScheduledItem(item));
```

- **Random Delays**: Items scheduled for sale with 1-4 hour delays
- **Automatic Sales**: Items sell automatically when scheduled time is reached
- **Time Tracking**: SellService tracks scheduled sale times

## UI Integration

### Time Display

```csharp
// In TimeLabelController
private void OnTimeChanged(GameTime time)
{
    _label.text = $"Day {time.Day}  {time.Hour:00}:{time.Minute:00}";
}
```

- **Real-Time Updates**: UI updates automatically when time changes
- **Event Subscription**: Controller subscribes to OnTimeChanged event
- **Formatting**: Time displayed in readable format
