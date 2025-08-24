using PawnShop.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PawnShop.Controllers
{
    public class TrainController : MonoBehaviour
    {
        [Header("Train Controls")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _stopButton;

        [Inject] private ITrainStateService _trainStateService;

        private void Start()
        {
            SetupButtons();
        }

        private void Update()
        {
            if (_trainStateService != null)
            {
                _trainStateService.Tick(Time.deltaTime);
            }
        }

        private void SetupButtons()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(StartTrain);

            if (_stopButton != null)
                _stopButton.onClick.AddListener(StopTrain);
        }

        private void StartTrain()
        {
            if (_trainStateService != null)
            {
                _trainStateService.StartDeparture();
            }
        }

        private void StopTrain()
        {
            if (_trainStateService != null)
            {
                _trainStateService.Stop();
            }
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_startButton != null)
                _startButton.onClick.RemoveAllListeners();

            if (_stopButton != null)
                _stopButton.onClick.RemoveAllListeners();
        }
    }
}
