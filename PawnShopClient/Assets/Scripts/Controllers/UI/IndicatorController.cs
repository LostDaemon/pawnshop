using TMPro;
using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    [SerializeField] private string _formatter = "{0}";
    [SerializeField] private float _animationSpeed = 300f;

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

        _displayValue = Mathf.MoveTowards(_displayValue, _targetValue, _animationSpeed * Time.deltaTime);
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