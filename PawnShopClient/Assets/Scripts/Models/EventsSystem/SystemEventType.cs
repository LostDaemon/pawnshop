namespace PawnShop.Models.EventsSystem
{
    /// <summary>
    /// Types of system events for internal game mechanics
    /// </summary>
    public enum SystemEventType
    {
        Undefined = 0,
        WorkDayStarted = 1, // Triggered at 8:00 AM to plan customers for the day
        WorkDayEnded = 2,   // Triggered at 18:00 PM to end work day
    }
}
