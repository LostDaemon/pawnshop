using PawnShop.Models;
using PawnShop.Models.EventsSystem;
using UnityEngine;
using Zenject;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Processor for system events
    /// </summary>
    public class SystemEventProcessor : EventProcessorBase<SystemEvent>
    {
        private readonly ITimeService _timeService;
        private readonly System.Random _random = new();

        [Inject]
        public SystemEventProcessor(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public override void ProcessEvent(SystemEvent systemEvent)
        {
            Debug.Log($"[SystemEventProcessor] TRIGGERED: {systemEvent.SystemEventType} at Day {systemEvent.Time.Day} {systemEvent.Time.Hour:00}:{systemEvent.Time.Minute:00}");

            switch (systemEvent.SystemEventType)
            {
                case SystemEventType.WorkDayStarted:
                    Debug.Log($"[SystemEventProcessor] WORK DAY STARTED - Planning customers for Day {systemEvent.Time.Day}");
                    ScheduleCustomersForDay(systemEvent.Time);
                    break;
                
                case SystemEventType.WorkDayEnded:
                    Debug.Log($"[SystemEventProcessor] WORK DAY ENDED - Day {systemEvent.Time.Day} finished");
                    break;
                
                default:
                    Debug.LogWarning($"[SystemEventProcessor] Unknown system event type: {systemEvent.SystemEventType}");
                    break;
            }
        }

        private void ScheduleCustomersForDay(GameTime dayStart, int customerCount = 10, int workDayStartHour = 9, int workDayEndHour = 18)
        {
            Debug.Log($"[SystemEventProcessor] SCHEDULING {customerCount} customers for Day {dayStart.Day} ({workDayStartHour:00}:00 - {workDayEndHour:00}:00)");
            
            // Generate random arrival times between workDayStartHour and workDayEndHour
            for (int i = 0; i < customerCount; i++)
            {
                var arrivalTime = GenerateRandomArrivalTime(dayStart, workDayStartHour, workDayEndHour);
                var customerEvent = new CustomerEvent(arrivalTime);
                
                _timeService.Schedule(customerEvent);
                Debug.Log($"[SystemEventProcessor] SCHEDULED: Customer #{i + 1} for Day {dayStart.Day} at {arrivalTime.Hour:00}:{arrivalTime.Minute:00}");
            }
            Debug.Log($"[SystemEventProcessor] All {customerCount} customers scheduled for Day {dayStart.Day}");
        }

        private GameTime GenerateRandomArrivalTime(GameTime dayStart, int workDayStartHour, int workDayEndHour)
        {
            // Generate random time between workDayStartHour and workDayEndHour
            int randomHour = _random.Next(workDayStartHour, workDayEndHour + 1); // inclusive range
            int randomMinute = _random.Next(0, 60); // 0 to 59
            
            return new GameTime(dayStart.Day, randomHour, randomMinute);
        }

        public override bool CanProcess(GameEventType eventType)
        {
            return eventType == GameEventType.SystemEvent;
        }
    }
}
