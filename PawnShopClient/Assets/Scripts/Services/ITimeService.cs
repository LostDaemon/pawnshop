using System;

public interface ITimeService
{
    GameTime CurrentTime { get; }
    float TimeMultiplier { get; set; } // 1x, 10x, etc

    event Action<GameTime> OnTimeChanged;

    void Tick(float deltaTime);
    void Schedule(GameTime time, Action callback);
}