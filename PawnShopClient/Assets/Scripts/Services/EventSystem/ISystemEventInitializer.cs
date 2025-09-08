namespace PawnShop.Services.EventSystem
{
    /// <summary>
    /// Service for initializing system events at game start
    /// </summary>
    public interface ISystemEventInitializer
    {
        /// <summary>
        /// Initialize all system events
        /// </summary>
        void InitializeSystemEvents();
    }
}
