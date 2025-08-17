using UnityEngine;
using TMPro;
using Zenject;

public class LocalizedTextController : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    
    private TextMeshPro _textComponent;
    private ILocalizationService _localizationService;
    
    [Inject]
    public void Construct(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _localizationService.OnLocalizationSwitch += UpdateLocalization;
    }
    
    private void Awake()
    {
        _textComponent = GetComponent<TextMeshPro>();
    }
    
    private void Start()
    {
        if (!string.IsNullOrEmpty(localizationKey))
        {
            UpdateLocalization();
        }
    }
    
    private void OnDestroy()
    {
        if (_localizationService != null)
        {
            _localizationService.OnLocalizationSwitch -= UpdateLocalization;
        }
    }
    
    public void SetLocalizationKey(string key)
    {
        localizationKey = key;
        UpdateLocalization();
    }
    
    public void UpdateLocalization()
    {
        if (_textComponent != null && !string.IsNullOrEmpty(localizationKey))
        {
            _textComponent.text = _localizationService.GetLocalization(localizationKey);
        }
    }
}
