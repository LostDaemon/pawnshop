namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Service for processing events from the queue
    /// </summary>
    public interface IEventProcessingService
    {
        /// <summary>
        /// Process all events currently in the queue
        /// </summary>
        void ProcessEvents();
    }
}
