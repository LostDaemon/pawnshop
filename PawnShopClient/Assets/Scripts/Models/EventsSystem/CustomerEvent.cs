using PawnShop.Models;

namespace PawnShop.Models.EventsSystem
{
    /// <summary>
    /// Event representing customer arrival at scheduled time
    /// </summary>
    public class CustomerEvent : GameEventBase
    {
        public CustomerEvent(GameTime arrivalTime)
        {
            Type = GameEventType.VisitorArrival;
            Time = arrivalTime;
        }
    }
}
