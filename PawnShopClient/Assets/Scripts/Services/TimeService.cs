using System;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Events;
using UnityEngine;

namespace PawnShop.Services
{
    public class TimeService : ITimeService
    {
        private const int MinutesInDay = 24 * 60;
        private const float SecondsPerMinute = 60f;
        private const float BaseTimeMultiplier = 60f;
        private float _timeAccumulator;

        public GameTime CurrentTime { get; private set; } = new(1, 8, 0); // Day 1, 08:00
        public float TimeMultiplier { get; set; } = BaseTimeMultiplier; // 1 real second = 1 in-game minute

        public event Action<GameTime> OnTimeChanged;
        public event Action<IGameEvent> OnEventTriggered;

        private readonly List<IGameEvent> _scheduledEvents = new();

        public void Tick(float deltaTime)
        {
            _timeAccumulator += deltaTime * TimeMultiplier;

            int minutesToAdvance = Mathf.FloorToInt(_timeAccumulator / SecondsPerMinute);
            for (int i = 0; i < minutesToAdvance; i++)
            {
                AdvanceMinute();
            }
            _timeAccumulator -= minutesToAdvance * SecondsPerMinute;
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

        public void Schedule(IGameEvent gameEvent)
        {
            _scheduledEvents.Add(gameEvent);
        }

        private void TriggerEvents(GameTime now)
        {
            for (int i = _scheduledEvents.Count - 1; i >= 0; i--)
            {
                if (IsSameOrPast(_scheduledEvents[i].Time, now))
                {
                    OnEventTriggered?.Invoke(_scheduledEvents[i]);
                    _scheduledEvents.RemoveAt(i);
                    
                    // Switch to 1x speed when any event triggers
                    TimeMultiplier = BaseTimeMultiplier;
                }
            }
        }

        private bool IsSameOrPast(GameTime a, GameTime b)
        {
            return a.Day < b.Day ||
                   (a.Day == b.Day && a.Hour < b.Hour) ||
                   (a.Day == b.Day && a.Hour == b.Hour && a.Minute <= b.Minute);
        }
    }
}