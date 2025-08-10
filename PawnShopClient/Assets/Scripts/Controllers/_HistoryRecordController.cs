using TMPro;
using UnityEngine;

//TODO: NOT USED
public class HistoryRecordController : MonoBehaviour
{
    private string _displayedText = string.Empty;
    private string _text = string.Empty;
    [SerializeField]
    [Range(0, 10)]
    private int _speed = 1;
    private void Start()
    {
        _label = GetComponentInChildren<TextMeshProUGUI>();
    }

    public string Text
    {
        get => _text; set
        {
            _displayedText = string.Empty;
            _text = value;
            _updated = false;
            _pos = 0;
        }
    }

    private TextMeshProUGUI _label;

    private int _pos = 0;
    private float _delay = 0;
    private bool _updated = false;


    private void LateUpdate()
    {
        _delay += Time.deltaTime * 1000; //ms
        if (_delay < (10 - _speed))
        {
            return;
        }
        Debug.Log($"Delay: {_delay}, Speed: {_speed}");
        _delay = 0;

        if (_updated) { return; }
        if (_displayedText != _text)
        {
            _pos++;
            if (_pos >= _text.Length)
            {
                _pos = _text.Length;
                _updated = true;
            }
            _displayedText = _text.Substring(0, _pos);
            _label.text = _displayedText;
        }
    }
}