
namespace PawnShop.Models.EventsSystem
{
    public interface IGameEvent
    {
        public GameEventType Type { get; set; }
        public GameTime Time { get; set; }
    }
}