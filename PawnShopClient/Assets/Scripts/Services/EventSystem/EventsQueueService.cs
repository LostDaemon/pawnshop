using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using PawnShop.Models.EventsSystem;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Implementation of event queue service - events are pushed by producers and pulled by consumers
    /// </summary>
    public class EventsQueueService : IEventsQueueService
    {
        private readonly List<IGameEvent> _eventQueue = new();
        
        public int QueueCount => _eventQueue.Count;
        
        [Inject]
        public EventsQueueService()
        {
            // Events will be pushed by producers (TimeService, etc.) and pulled by consumers
        }
        
        public void Push(IGameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                Debug.LogWarning("EventsQueueService: Attempted to push null game event");
                return;
            }
            
            _eventQueue.Add(gameEvent);
            Debug.Log($"EventsQueueService: Event {gameEvent.Type} pushed to queue. Queue size: {_eventQueue.Count}");
        }
        
        public IGameEvent Pull()
        {
            if (_eventQueue.Count == 0)
            {
                return null;
            }
            
            var gameEvent = _eventQueue[0];
            _eventQueue.RemoveAt(0);
            Debug.Log($"EventsQueueService: Event {gameEvent.Type} pulled from queue. Remaining: {_eventQueue.Count}");
            return gameEvent;
        }
        
        public IGameEvent Pull(GameEventType eventType)
        {
            if (_eventQueue.Count == 0)
            {
                return null;
            }
            
            // Find first event of specified type
            for (int i = 0; i < _eventQueue.Count; i++)
            {
                if (_eventQueue[i].Type == eventType)
                {
                    var gameEvent = _eventQueue[i];
                    _eventQueue.RemoveAt(i);
                    Debug.Log($"EventsQueueService: Event {eventType} pulled from queue. Remaining: {_eventQueue.Count}");
                    return gameEvent;
                }
            }
            
            Debug.Log($"EventsQueueService: No events of type {eventType} found in queue");
            return null;
        }
        
        public void ClearQueue()
        {
            _eventQueue.Clear();
            Debug.Log("EventsQueueService: Queue cleared");
        }
        
        public int RemoveEventsOfType(GameEventType eventType)
        {
            var initialCount = _eventQueue.Count;
            
            // Remove events of specified type (iterate backwards to avoid index issues)
            for (int i = _eventQueue.Count - 1; i >= 0; i--)
            {
                if (_eventQueue[i].Type == eventType)
                {
                    _eventQueue.RemoveAt(i);
                }
            }
            
            var removedCount = initialCount - _eventQueue.Count;
            if (removedCount > 0)
            {
                Debug.Log($"EventsQueueService: Removed {removedCount} events of type {eventType}. Remaining: {_eventQueue.Count}");
            }
            
            return removedCount;
        }
        
        public IReadOnlyList<IGameEvent> GetPendingEvents()
        {
            return _eventQueue.AsReadOnly();
        }
        
        public IReadOnlyList<IGameEvent> GetPendingEventsOfType(GameEventType eventType)
        {
            return _eventQueue.Where(e => e.Type == eventType).ToList().AsReadOnly();
        }
    }
}
