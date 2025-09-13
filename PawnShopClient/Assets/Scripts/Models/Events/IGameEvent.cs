using System;
using PawnShop.Models;

namespace PawnShop.Models.Events
{
    public interface IGameEvent
    {
        GameEventType EventType { get; set; }
        GameTime Time { get; set; }
    }
}
