using PawnShop.Services;
using PawnShop.Models.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace PawnShop.Controllers
{
    public class TimeDriverController : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _normalSpeedButton;
        [SerializeField] private Button _fastSpeedButton;
        [SerializeField] private Button _skipTimeButton;

        [Inject] private ITimeService _timeService;

        private Button _currentActiveButton;
        private Color _normalColor = new Color(0.196f, 0.196f, 0.196f); 
        private Color _activeColor = new Color(1f, 0.5f, 0f); // Orange

        private void Start()
        {
            SetupButtons();
            _timeService.OnEventTriggered += OnEventTriggered;
        }

        private void OnDestroy()
        {
            if (_timeService != null)
            {
                _timeService.OnEventTriggered -= OnEventTriggered;
            }
        }

        private void OnEventTriggered(IGameEvent gameEvent)
        {
            SetActiveButton(_normalSpeedButton);
        }

        private void SetupButtons()
        {
            _pauseButton?.onClick.AddListener(() => SetTimeMultiplier(0f, _pauseButton));
            _normalSpeedButton?.onClick.AddListener(() => SetTimeMultiplier(1f, _normalSpeedButton));
            _fastSpeedButton?.onClick.AddListener(() => SetTimeMultiplier(4f, _fastSpeedButton));
            _skipTimeButton?.onClick.AddListener(() => SetTimeMultiplier(16f, _skipTimeButton));
        }

        private void SetTimeMultiplier(float multiplier, Button button)
        {
            _timeService.TimeMultiplier = multiplier * 60f;
            Debug.Log($"[TimeDriverController] Time multiplier set to: {multiplier}x ({multiplier * 60f})");
            
            SetActiveButton(button);
        }

        private void SetActiveButton(Button activeButton)
        {
            Debug.Log($"[TimeDriverController] SetActiveButton called with button: {activeButton?.name}");
            
            // Reset previous button color
            if (_currentActiveButton != null)
            {
                var text = _currentActiveButton.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"[TimeDriverController] Previous button text found: {text != null}");
                if (text != null)
                {
                    text.color = _normalColor;
                    Debug.Log($"[TimeDriverController] Previous button color set to normal");
                }
            }
            
            // Set new active button color
            if (activeButton != null)
            {
                var text = activeButton.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"[TimeDriverController] New button text found: {text != null}");
                if (text != null)
                {
                    text.color = _activeColor;
                    Debug.Log($"[TimeDriverController] New button color set to active");
                }
                _currentActiveButton = activeButton;
            }
        }

        private void Update()
        {
            _timeService.Tick(Time.deltaTime);
        }
    }
}