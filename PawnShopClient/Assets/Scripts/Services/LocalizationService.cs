using System.Collections.Generic;
using UnityEngine;

public class LocalizationService : ILocalizationService
{
    private readonly ILanguageRepositoryService _languageRepository;
    private Dictionary<string, string[]> _localizationDictionary;
    private Language _currentLanguage;
    
    public event System.Action OnLocalizationSwitch;

    public LocalizationService(ILanguageRepositoryService languageRepository)
    {
        _languageRepository = languageRepository;
        _localizationDictionary = new Dictionary<string, string[]>();
    }

    public void SwitchLocalization(Language language)
    {
        var languagePrototype = _languageRepository.GetLanguage(language);
        if (languagePrototype == null)
        {
            Debug.LogWarning($"Language {language} not found in prototypes");
            return;
        }

        LoadLocalizationFile(languagePrototype.fileName);
        _currentLanguage = language;
        OnLocalizationSwitch?.Invoke();
    }

    public string GetLocalization(string key)
    {
        if (_localizationDictionary.TryGetValue(key, out string[] values))
        {
            if (values == null || values.Length == 0)
            {
                Debug.LogWarning($"Localization key '{key}' has no values for language {_currentLanguage}");
                return $"[{key}]";
            }
            
            // If only one value, return it directly
            if (values.Length == 1)
            {
                return values[0];
            }
            
            // If multiple values, return a random one
            return values[Random.Range(0, values.Length)];
        }
        
        Debug.LogWarning($"Localization key '{key}' not found for language {_currentLanguage}");
        return $"[{key}]";
    }

    private void LoadLocalizationFile(string fileName)
    {
        _localizationDictionary.Clear();
        
        var textAsset = Resources.Load<TextAsset>($"L10n/{fileName}");
        if (textAsset == null)
        {
            Debug.LogError($"Localization file not found: L10n/{fileName}");
            return;
        }

        try
        {
            var jsonContent = textAsset.text;
            var localizationData = JsonUtility.FromJson<LocalizationData>(jsonContent);
            
            foreach (var entry in localizationData.entries)
            {
                _localizationDictionary[entry.key] = entry.values;
                Debug.Log($"Loaded localization entry: {entry.key} = {entry.values[0]}");
            }
            
            Debug.Log($"Loaded {_localizationDictionary.Count} localization entries from {fileName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load localization file {fileName}: {e.Message}");
        }
    }

    [System.Serializable]
    private class LocalizationData
    {
        public LocalizationEntry[] entries;
    }

    [System.Serializable]
    private class LocalizationEntry
    {
        public string key;
        public string[] values;
    }
}
