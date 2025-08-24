using System;

namespace PawnShop.Services
{
    public enum TrainState
    {
        Stopped,
        Departing,
        Moving,
        Arriving,
    }

    public interface ITrainStateService
    {
        TrainState CurrentState { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
        float Acceleration { get; }
        float Deceleration { get; }

        event Action<TrainState> OnStateChanged;
        event Action<float> OnSpeedChanged;
        event Action OnDepartureStarted;
        event Action OnArrivalCompleted;

        void StartDeparture();
        void StartArrival();
        void Stop();
        void Tick(float deltaTime);
    }
}
