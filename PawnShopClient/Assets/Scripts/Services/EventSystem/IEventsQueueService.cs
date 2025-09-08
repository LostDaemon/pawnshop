using System;
using System.Collections.Generic;
using PawnShop.Models.EventsSystem;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Service for managing event queue - events are pulled by producers
    /// </summary>
    public interface IEventsQueueService
    {
        /// <summary>
        /// Current number of events in queue
        /// </summary>
        int QueueCount { get; }
        
        /// <summary>
        /// Add event to queue for processing
        /// </summary>
        /// <param name="gameEvent">Game event to execute</param>
        void Push(IGameEvent gameEvent);
        
        /// <summary>
        /// Pull next event from queue for processing
        /// </summary>
        /// <returns>Next event to process or null if queue is empty</returns>
        IGameEvent Pull();
        
        /// <summary>
        /// Pull next event of specific type from queue for processing
        /// </summary>
        /// <param name="eventType">Type of event to pull</param>
        /// <returns>Next event of specified type or null if no such event exists</returns>
        IGameEvent Pull(GameEventType eventType);
        
        /// <summary>
        /// Clear all pending events from queue
        /// </summary>
        void ClearQueue();
        
        /// <summary>
        /// Remove all events of specific type from queue
        /// </summary>
        /// <param name="eventType">Type of events to remove</param>
        /// <returns>Number of events removed</returns>
        int RemoveEventsOfType(GameEventType eventType);
        
        /// <summary>
        /// Get all pending events in queue (for debugging)
        /// </summary>
        IReadOnlyList<IGameEvent> GetPendingEvents();
        
        /// <summary>
        /// Get all pending events of specific type in queue
        /// </summary>
        /// <param name="eventType">Type of events to get</param>
        /// <returns>List of events of specified type</returns>
        IReadOnlyList<IGameEvent> GetPendingEventsOfType(GameEventType eventType);
    }
}
