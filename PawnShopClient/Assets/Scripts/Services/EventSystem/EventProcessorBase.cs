using PawnShop.Models.EventsSystem;

namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Abstract base class for event processors
    /// </summary>
    /// <typeparam name="TEvent">Type of event this processor handles</typeparam>
    public abstract class EventProcessorBase<TEvent> where TEvent : IGameEvent
    {
        /// <summary>
        /// Process the event
        /// </summary>
        /// <param name="gameEvent">Event to process</param>
        public abstract void ProcessEvent(TEvent gameEvent);
        
        /// <summary>
        /// Check if this processor can handle the given event type
        /// </summary>
        /// <param name="eventType">Type of event to check</param>
        /// <returns>True if this processor can handle the event type</returns>
        public abstract bool CanProcess(GameEventType eventType);
    }
}
