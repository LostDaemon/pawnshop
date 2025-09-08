using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.EventsSystem;
using PawnShop.Services.EventSystem;
using UnityEngine;
using Zenject;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Main service for processing events from the queue
    /// </summary>
    public class EventProcessingService : IEventProcessingService
    {
        private readonly IEventsQueueService _eventsQueueService;
        private readonly ITimeService _timeService;
        private readonly List<EventProcessorBase<SystemEvent>> _systemEventProcessors;
        private readonly List<EventProcessorBase<CustomerEvent>> _customerEventProcessors;

        [Inject]
        public EventProcessingService(
            IEventsQueueService eventsQueueService,
            ITimeService timeService,
            EventProcessorBase<SystemEvent>[] systemEventProcessors,
            EventProcessorBase<CustomerEvent>[] customerEventProcessors)
        {
            _eventsQueueService = eventsQueueService;
            _timeService = timeService;
            _systemEventProcessors = systemEventProcessors.ToList();
            _customerEventProcessors = customerEventProcessors.ToList();
            
            // Subscribe to time changes to automatically process events
            _timeService.OnTimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(GameTime newTime)
        {
            // Process events every time the game time changes
            ProcessEvents();
        }

        public void ProcessEvents()
        {
            // Process all events in the queue
            while (_eventsQueueService.QueueCount > 0)
            {
                var gameEvent = _eventsQueueService.Pull();
                if (gameEvent == null) break;

                ProcessEvent(gameEvent);
            }
        }

        private void ProcessEvent(IGameEvent gameEvent)
        {
            Debug.Log($"[EventProcessingService] Processing event: {gameEvent.Type} at {gameEvent.Time.Hour:00}:{gameEvent.Time.Minute:00}");

            switch (gameEvent.Type)
            {
                case GameEventType.SystemEvent:
                    ProcessSystemEvent(gameEvent as SystemEvent);
                    break;
                
                case GameEventType.VisitorArrival:
                    ProcessCustomerEvent(gameEvent as CustomerEvent);
                    break;
                
                case GameEventType.CustomVisitorArrival:
                    ProcessCustomVisitorEvent(gameEvent);
                    break;
                
                default:
                    Debug.LogWarning($"[EventProcessingService] Unknown event type: {gameEvent.Type}");
                    break;
            }
        }

        private void ProcessSystemEvent(SystemEvent systemEvent)
        {
            if (systemEvent == null) 
            {
                Debug.LogWarning("[EventProcessingService] SystemEvent is null");
                return;
            }

            Debug.Log($"[EventProcessingService] Processing SystemEvent: {systemEvent.SystemEventType} with {_systemEventProcessors.Count} processors");

            foreach (var processor in _systemEventProcessors)
            {
                Debug.Log($"[EventProcessingService] Checking processor: {processor.GetType().Name}");
                if (processor.CanProcess(systemEvent.Type))
                {
                    Debug.Log($"[EventProcessingService] Calling ProcessEvent on {processor.GetType().Name}");
                    processor.ProcessEvent(systemEvent);
                }
                else
                {
                    Debug.Log($"[EventProcessingService] Processor {processor.GetType().Name} cannot process {systemEvent.Type}");
                }
            }
        }

        private void ProcessCustomerEvent(CustomerEvent customerEvent)
        {
            if (customerEvent == null) return;

            foreach (var processor in _customerEventProcessors)
            {
                if (processor.CanProcess(customerEvent.Type))
                {
                    processor.ProcessEvent(customerEvent);
                }
            }
        }

        private void ProcessCustomVisitorEvent(IGameEvent customEvent)
        {
            Debug.Log($"[EventProcessingService] Processing custom visitor event at {customEvent.Time.Hour:00}:{customEvent.Time.Minute:00}");
            // Handle custom visitor events if needed
        }

        /// <summary>
        /// Cleanup method to unsubscribe from events
        /// </summary>
        public void Dispose()
        {
            if (_timeService != null)
            {
                _timeService.OnTimeChanged -= OnTimeChanged;
            }
        }
    }
}
