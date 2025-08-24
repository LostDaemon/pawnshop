using System;
using UnityEngine;

namespace PawnShop.Services
{
    public class TrainStateService : ITrainStateService
    {
        private TrainState _currentState = TrainState.Stopped;
        private float _currentSpeed = 0f;


        public TrainState CurrentState => _currentState;
        public float CurrentSpeed => _currentSpeed;
        public float MaxSpeed { get; private set; } = 30f; // Units per second
        public float Acceleration { get; private set; } = 5f; // Units per second squared
        public float Deceleration { get; private set; } = 8f; // Units per second squared

        public event Action<TrainState> OnStateChanged;
        public event Action<float> OnSpeedChanged;
        public event Action OnDepartureStarted;
        public event Action OnArrivalCompleted;

        public void StartDeparture()
        {
            // Can start from any state except already departing
            if (_currentState == TrainState.Departing)
            {
                Debug.LogWarning("Train is already departing");
                return;
            }

            ChangeState(TrainState.Departing);
            OnDepartureStarted?.Invoke();
        }

        public void StartArrival()
        {
            // Can start arrival from any state except already arriving
            if (_currentState == TrainState.Arriving)
            {
                Debug.LogWarning("Train is already arriving");
                return;
            }

            ChangeState(TrainState.Arriving);
        }



        public void Stop()
        {
            // Can stop from any state except already arriving
            if (_currentState != TrainState.Arriving)
            {
                StartArrival();
            }
        }

        public void Tick(float deltaTime)
        {
            switch (_currentState)
            {
                case TrainState.Stopped:
                    HandleStoppedState(deltaTime);
                    break;
                case TrainState.Departing:
                    HandleDepartingState(deltaTime);
                    break;
                case TrainState.Moving:
                    HandleMovingState(deltaTime);
                    break;
                case TrainState.Arriving:
                    HandleArrivingState(deltaTime);
                    break;
            }
        }

        private void HandleStoppedState(float deltaTime)
        {
            // Train is stopped, no movement
            if (_currentSpeed > 0f)
            {
                _currentSpeed = 0f;
                OnSpeedChanged?.Invoke(_currentSpeed);
            }
        }

        private void HandleDepartingState(float deltaTime)
        {
            // Gradually accelerate towards max speed
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, MaxSpeed, Acceleration * deltaTime);
            OnSpeedChanged?.Invoke(_currentSpeed);
            
            // When reaching max speed, switch to Moving state
            if (Mathf.Approximately(_currentSpeed, MaxSpeed))
            {
                ChangeState(TrainState.Moving);
            }
        }

        private void HandleMovingState(float deltaTime)
        {
            // Maintain max speed
            _currentSpeed = MaxSpeed;
            OnSpeedChanged?.Invoke(_currentSpeed);
        }

        private void HandleArrivingState(float deltaTime)
        {
            // Gradually reduce speed to 0
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, Deceleration * deltaTime);
            OnSpeedChanged?.Invoke(_currentSpeed);

            // When speed reaches 0, switch to Stopped state
            if (Mathf.Approximately(_currentSpeed, 0f))
            {
                _currentSpeed = 0f;
                ChangeState(TrainState.Stopped);
                OnArrivalCompleted?.Invoke();
            }
        }

        private void ChangeState(TrainState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                OnStateChanged?.Invoke(newState);
                Debug.Log($"Train state changed to: {newState}");
            }
        }

        // Configuration methods
        public void SetMaxSpeed(float maxSpeed)
        {
            MaxSpeed = Mathf.Max(0f, maxSpeed);
        }

        public void SetAcceleration(float acceleration)
        {
            Acceleration = Mathf.Max(0f, acceleration);
        }

        public void SetDeceleration(float deceleration)
        {
            Deceleration = Mathf.Max(0f, deceleration);
        }
    }
}
