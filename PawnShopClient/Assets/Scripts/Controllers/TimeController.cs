using PawnShop.Models;
using PawnShop.Services;
using TMPro;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class TimeLabelController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        [Inject] private ITimeService _timeService;

        private void Start()
        {
            if (_timeService == null || _label == null)
            {
                Debug.LogError("TimeService or Label not assigned.");
                return;
            }

            _timeService.OnTimeChanged += OnTimeChanged;
            OnTimeChanged(_timeService.CurrentTime); // immediate update
        }

        private void OnDestroy()
        {
            if (_timeService != null)
                _timeService.OnTimeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(GameTime time)
        {
            _label.text = $"Day {time.Day}  {time.Hour:00}:{time.Minute:00}";
        }
    }
}