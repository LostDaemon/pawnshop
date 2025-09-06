using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    /// <summary>
    /// Implementation of event queue service for managing concurrent event execution
    /// </summary>
    public class EventsQueueService : IEventsQueueService
    {
        private readonly Queue<Action> _eventQueue = new();
        private bool _isProcessing = false;
        private readonly ITimeService _timeService;
        
        public int QueueCount => _eventQueue.Count;
        public bool IsProcessing => _isProcessing;
        
        public event Action<Action> OnEventStarted;
        public event Action<Action> OnEventCompleted;
        
        [Inject]
        public EventsQueueService(ITimeService timeService)
        {
            _timeService = timeService;
            
            // Subscribe to time service events
            _timeService.OnEventTriggered += OnTimeServiceEventTriggered;
        }
        
        private void OnTimeServiceEventTriggered(Action eventCallback)
        {
            // When TimeService triggers an event, add it to our queue
            EnqueueEvent(eventCallback);
        }
        
        public void EnqueueEvent(Action eventCallback)
        {
            if (eventCallback == null)
            {
                Debug.LogWarning("EventsQueueService: Attempted to enqueue null event callback");
                return;
            }
            
            _eventQueue.Enqueue(eventCallback);
            Debug.Log($"EventsQueueService: Event enqueued from TimeService. Queue size: {_eventQueue.Count}");
            
            // Try to process immediately if no event is currently processing
            if (!_isProcessing)
            {
                ProcessNextEvent();
            }
        }
        
        public void ProcessNextEvent()
        {
            if (_isProcessing)
            {
                Debug.Log("EventsQueueService: Cannot process next event, already processing one");
                return;
            }
            
            if (_eventQueue.Count == 0)
            {
                Debug.Log("EventsQueueService: No events in queue to process");
                return;
            }
            
            var eventCallback = _eventQueue.Dequeue();
            _isProcessing = true;
            
            Debug.Log($"EventsQueueService: Starting event processing. Remaining in queue: {_eventQueue.Count}");
            OnEventStarted?.Invoke(eventCallback);
            
            try
            {
                eventCallback.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventsQueueService: Error processing event: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
                Debug.Log("EventsQueueService: Event processing completed");
                OnEventCompleted?.Invoke(eventCallback);
                
                // Process next event if there are any in queue
                if (_eventQueue.Count > 0)
                {
                    ProcessNextEvent();
                }
            }
        }
        
        public void ClearQueue()
        {
            _eventQueue.Clear();
            Debug.Log("EventsQueueService: Queue cleared");
        }
        
        public IReadOnlyList<Action> GetPendingEvents()
        {
            return _eventQueue.ToList().AsReadOnly();
        }
    }
}