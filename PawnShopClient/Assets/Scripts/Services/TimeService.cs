using System;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.EventsSystem;
using PawnShop.Services.EventSystem;
using Zenject;

namespace PawnShop.Services
{
    public partial class TimeService : ITimeService
    {
        private const int MinutesInDay = 24 * 60;
        private float _timeAccumulator;
        private readonly IEventsQueueService _eventsQueueService;

        public GameTime CurrentTime { get; private set; }
        public float TimeMultiplier { get; set; } = 60f; // 1 real second = 1 in-game minute

        public event Action<GameTime> OnTimeChanged;

        private readonly List<IGameEvent> _scheduledEvents = new();

        [Inject]
        public TimeService(IEventsQueueService eventsQueueService, GameTime initialTime)
        {
            _eventsQueueService = eventsQueueService;
            CurrentTime = initialTime;
        }

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
                    var eventToTrigger = _scheduledEvents[i];
                    // Push event to queue instead of firing event
                    _eventsQueueService.Push(eventToTrigger);
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
    }
}