using TMPro;
using UnityEngine;

namespace PawnShop.Controllers
{
    public class IndicatorController : MonoBehaviour
    {
        [SerializeField] private string _formatter = "{0}";
        [Tooltip("Maximum animation duration in seconds.")]
        [Range(0.01f, 2f)]
        [SerializeField] private float _maxAnimationDuration = 1f;

        private TextMeshProUGUI _label;
        private float _targetValue = 0;
        private float _displayValue = 0;

        private void Start()
        {
            _label = GetComponentInChildren<TextMeshProUGUI>();
            if (_label == null)
            {
                Debug.LogError("IndicatorController: TextMeshProUGUI not found.");
            }
            else
            {
                UpdateLabel();
            }
        }

        public void SetValue(float value, bool animate = true)
        {
            _targetValue = value;
            if (!animate)
            {
                _displayValue = value;
                UpdateLabel();
            }
        }

        private void LateUpdate()
        {
            if (Mathf.Approximately(_displayValue, _targetValue))
                return;

            float distance = Mathf.Abs(_targetValue - _displayValue);
            float speed = distance / _maxAnimationDuration;

            _displayValue = Mathf.MoveTowards(_displayValue, _targetValue, speed * Time.deltaTime);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (_label != null)
            {
                _label.text = string.Format(_formatter, Mathf.RoundToInt(_displayValue));
            }
        }
    }
}