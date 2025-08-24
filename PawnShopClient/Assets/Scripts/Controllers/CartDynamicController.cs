using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class CartDynamicController : MonoBehaviour
    {
        [Header("Cart Components")]
        [SerializeField] private Transform _body; // Main body of the cart
        [SerializeField] private Transform[] _wheels = new Transform[4]; // 4 wheels

        [Header("Body Rocking Settings")]
        [SerializeField] private float _minSpeedForRocking = 5f; // Speed at which rocking starts
        [SerializeField] private float _rockingAmplitude = 0.1f; // Vertical rocking amplitude
        [SerializeField] private float _rockingFrequency = 2f; // Rocking frequency in Hz
        [SerializeField] private float _maxRockingAmplitude = 0.3f; // Maximum rocking at max speed

        [Header("Wheel Rotation Settings")]
        [SerializeField] private float _wheelRadius = 0.5f; // Wheel radius for rotation calculation

        [Inject] private ITrainStateService _trainStateService;

        private Vector3 _bodyInitialPosition;
        private float _randomPhase; // Random phase for different carts
        private float _currentRockingAmplitude;

        private void Start()
        {
            InitializeCart();
        }

        private void InitializeCart()
        {
            if (_body != null)
            {
                _bodyInitialPosition = _body.localPosition;
            }

            // Random phase for different carts to rock out of sync
            _randomPhase = Random.Range(0f, 2f * Mathf.PI);
        }

        private void Update()
        {
            if (_trainStateService == null) return;

            UpdateBodyRocking();
            UpdateWheelRotation();
        }

        private void UpdateBodyRocking()
        {
            if (_body == null) return;

            float currentSpeed = _trainStateService.CurrentSpeed;
            
            // Only rock if speed is above minimum threshold
            if (currentSpeed >= _minSpeedForRocking)
            {
                // Calculate rocking amplitude based on speed
                float speedRatio = Mathf.Clamp01((currentSpeed - _minSpeedForRocking) / (_trainStateService.MaxSpeed - _minSpeedForRocking));
                _currentRockingAmplitude = Mathf.Lerp(0f, _maxRockingAmplitude, speedRatio);

                // Apply harmonic rocking with random phase
                float rockingOffset = _currentRockingAmplitude * Mathf.Sin(2f * Mathf.PI * _rockingFrequency * Time.time + _randomPhase);
                
                Vector3 newPosition = _bodyInitialPosition + Vector3.up * rockingOffset;
                _body.localPosition = newPosition;
            }
            else
            {
                // Return to initial position when not rocking
                _body.localPosition = _bodyInitialPosition;
            }
        }

        private void UpdateWheelRotation()
        {
            if (_wheels == null || _wheels.Length == 0) return;

            float currentSpeed = _trainStateService.CurrentSpeed;
            
            // Calculate wheel rotation based on speed and time
            float wheelCircumference = 2f * Mathf.PI * _wheelRadius;
            float rotationSpeed = currentSpeed / wheelCircumference; // Rotations per second
            
            foreach (Transform wheel in _wheels)
            {
                if (wheel != null)
                {
                    // Rotate wheel around local Z axis (forward-backward rotation)
                    float currentRotation = wheel.localEulerAngles.z;
                    float newRotation = currentRotation + rotationSpeed * 360f * Time.deltaTime;
                    
                    // Keep rotation in 0-360 range
                    newRotation = newRotation % 360f;
                    
                    wheel.localEulerAngles = new Vector3(wheel.localEulerAngles.x, wheel.localEulerAngles.y, newRotation);
                }
            }
        }

        // Public methods for external control
        public void SetRockingAmplitude(float amplitude)
        {
            _rockingAmplitude = Mathf.Clamp(amplitude, 0f, _maxRockingAmplitude);
        }

        public void SetRockingFrequency(float frequency)
        {
            _rockingFrequency = Mathf.Max(0f, frequency);
        }

        public void SetMinSpeedForRocking(float minSpeed)
        {
            _minSpeedForRocking = Mathf.Max(0f, minSpeed);
        }

        public void SetWheelRadius(float radius)
        {
            _wheelRadius = Mathf.Max(0.1f, radius);
        }

        // Get current rocking state
        public bool IsRocking => _trainStateService != null && _trainStateService.CurrentSpeed >= _minSpeedForRocking;
        public float CurrentRockingAmplitude => _currentRockingAmplitude;
    }
}
