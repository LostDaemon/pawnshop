using System;
using PawnShop.Models;
using PawnShop.Models.EventsSystem;

namespace PawnShop.Services
{
    public interface ITimeService
    {
        GameTime CurrentTime { get; }
        float TimeMultiplier { get; set; } // 1x, 10x, etc

        event Action<GameTime> OnTimeChanged;

        void Tick(float deltaTime);
        void Schedule(IGameEvent gameEvent);
    }
}