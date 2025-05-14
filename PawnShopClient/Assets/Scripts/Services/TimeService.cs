using System;
using System.Collections.Generic;

public class TimeService : ITimeService
{
    private const int MinutesInDay = 24 * 60;
    private float _timeAccumulator;

    public GameTime CurrentTime { get; private set; } = new(1, 8, 0); // Day 1, 08:00
    public float TimeMultiplier { get; set; } = 60f; // 1 real second = 1 in-game minute

    public event Action<GameTime> OnTimeChanged;

    private readonly List<ScheduledEvent> _scheduledEvents = new();

    public void Tick(float deltaTime)
    {
        _timeAccumulator += deltaTime * TimeMultiplier;

        while (_timeAccumulator >= 60f)
        {
            AdvanceMinute();
            _timeAccumulator -= 60f;
        }
    }

    private void AdvanceMinute()
    {
        CurrentTime = AddMinutes(CurrentTime, 1);
        OnTimeChanged?.Invoke(CurrentTime);
        TriggerEvents(CurrentTime);
    }

    private GameTime AddMinutes(GameTime time, int minutesToAdd)
    {
        int totalMinutes = time.Hour * 60 + time.Minute + minutesToAdd;
        int newDay = time.Day + totalMinutes / MinutesInDay;
        totalMinutes %= MinutesInDay;

        return new GameTime(newDay, totalMinutes / 60, totalMinutes % 60);
    }

    public void Schedule(GameTime time, Action callback)
    {
        _scheduledEvents.Add(new ScheduledEvent { Time = time, Callback = callback });
    }

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

    private bool IsSameOrPast(GameTime a, GameTime b)
    {
        return a.Day < b.Day ||
               (a.Day == b.Day && a.Hour < b.Hour) ||
               (a.Day == b.Day && a.Hour == b.Hour && a.Minute <= b.Minute);
    }

    private class ScheduledEvent
    {
        public GameTime Time;
        public Action Callback;
    }
}