namespace PawnShop.Models.EventsSystem
{
    public abstract class GameEventBase : IGameEvent
    {
        public GameEventType Type { get; set; }
        public GameTime Time { get; set; }
    }
}