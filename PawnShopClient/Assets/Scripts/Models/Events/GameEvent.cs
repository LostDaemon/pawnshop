using System;
using PawnShop.Models;

namespace PawnShop.Models.Events
{
    public class GameEvent : IGameEvent
    {
        public GameEventType EventType { get; set; }
        public GameTime Time { get; set; }
    }
}
