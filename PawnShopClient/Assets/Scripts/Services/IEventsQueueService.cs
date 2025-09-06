using System;
using System.Collections.Generic;

namespace PawnShop.Services
{
    /// <summary>
    /// Service for managing event queue to handle concurrent event execution
    /// </summary>
    public interface IEventsQueueService
    {
        /// <summary>
        /// Current number of events in queue
        /// </summary>
        int QueueCount { get; }
        
        /// <summary>
        /// Whether an event is currently being processed
        /// </summary>
        bool IsProcessing { get; }
        
        /// <summary>
        /// Event fired when an event starts processing
        /// </summary>
        event Action<Action> OnEventStarted;
        
        /// <summary>
        /// Event fired when an event finishes processing
        /// </summary>
        event Action<Action> OnEventCompleted;
        
        /// <summary>
        /// Add event to queue for processing
        /// </summary>
        /// <param name="eventCallback">Event callback to execute</param>
        void EnqueueEvent(Action eventCallback);
        
        /// <summary>
        /// Process next event in queue if no event is currently processing
        /// </summary>
        void ProcessNextEvent();
        
        /// <summary>
        /// Clear all pending events from queue
        /// </summary>
        void ClearQueue();
        
        /// <summary>
        /// Get all pending events in queue (for debugging)
        /// </summary>
        IReadOnlyList<Action> GetPendingEvents();
    }
}
