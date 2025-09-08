using PawnShop.Models;
using PawnShop.Models.EventsSystem;
using UnityEngine;
using Zenject;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Service for initializing system events at game start
    /// </summary>
    public class SystemEventInitializer : ISystemEventInitializer
    {
        private readonly ITimeService _timeService;

        [Inject]
        public SystemEventInitializer(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public void InitializeSystemEvents()
        {
            Debug.Log("[SystemEventInitializer] Initializing system events");
            
            // Schedule work day events for 8:00 AM and 18:00 PM for current day only
            var currentTime = _timeService.CurrentTime;
            
            // Schedule for current day if it's before 8:00 AM
            if (currentTime.Hour <= 8)
            {
                var workDayStartEvent = new SystemEvent(
                    new GameTime(currentTime.Day, 8, 0), 
                    SystemEventType.WorkDayStarted);
                _timeService.Schedule(workDayStartEvent);
                Debug.Log($"[SystemEventInitializer] SCHEDULED: WorkDayStarted for Day {currentTime.Day} at 08:00");
                
                var workDayEndEvent = new SystemEvent(
                    new GameTime(currentTime.Day, 18, 0), 
                    SystemEventType.WorkDayEnded);
                _timeService.Schedule(workDayEndEvent);
                Debug.Log($"[SystemEventInitializer] SCHEDULED: WorkDayEnded for Day {currentTime.Day} at 18:00");
            }
            else
            {
                Debug.Log($"[SystemEventInitializer] Current time is {currentTime.Hour:00}:{currentTime.Minute:00} - no events scheduled (already past 8:00 AM)");
            }
        }
    }
}
