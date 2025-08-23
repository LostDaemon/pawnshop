using PawnShop.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PawnShop.Controllers
{
    public class TimeDriverController : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _speed1xButton;
        [SerializeField] private Button _speed2xButton;
        [SerializeField] private Button _speed5xButton;
        [SerializeField] private Button _speed10xButton;

        [Inject] private ITimeService _timeService;

        private bool _isPaused = false;
        private float _normalTimeMultiplier;

        private void Start()
        {
            if (_timeService != null)
            {
                _normalTimeMultiplier = _timeService.TimeMultiplier;
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(TogglePause);

            if (_speed1xButton != null)
                _speed1xButton.onClick.AddListener(() => SetSpeed(1f));

            if (_speed2xButton != null)
                _speed2xButton.onClick.AddListener(() => SetSpeed(2f));

            if (_speed5xButton != null)
                _speed5xButton.onClick.AddListener(() => SetSpeed(5f));

            if (_speed10xButton != null)
                _speed10xButton.onClick.AddListener(() => SetSpeed(10f));
        }

        private void Update()
        {
            if (!_isPaused)
            {
                _timeService.Tick(Time.deltaTime);
            }
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Debug.Log("Time paused");
            }
            else
            {
                Debug.Log("Time resumed");
            }

        }

        public void SetSpeed(float multiplier)
        {
            if (_timeService != null)
            {
                _timeService.TimeMultiplier = _normalTimeMultiplier * multiplier;
                Debug.Log($"Time speed set to {multiplier}x");
            }
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public bool IsPaused => _isPaused;

        public float GetCurrentSpeed()
        {
            if (_timeService != null)
            {
                return _timeService.TimeMultiplier / _normalTimeMultiplier;
            }
            return 1f;
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveAllListeners();

            if (_speed1xButton != null)
                _speed1xButton.onClick.RemoveAllListeners();

            if (_speed2xButton != null)
                _speed2xButton.onClick.RemoveAllListeners();

            if (_speed5xButton != null)
                _speed5xButton.onClick.RemoveAllListeners();

            if (_speed10xButton != null)
                _speed10xButton.onClick.RemoveAllListeners();
        }
    }
}