using PawnShop.Models;

namespace PawnShop.Models.EventsSystem
{
    /// <summary>
    /// System event for internal game mechanics like daily planning
    /// </summary>
    public class SystemEvent : GameEventBase
    {
        public SystemEventType SystemEventType { get; set; }
        
        public SystemEvent(GameTime eventTime, SystemEventType systemEventType)
        {
            Type = GameEventType.SystemEvent;
            Time = eventTime;
            SystemEventType = systemEventType;
        }
    }
}
