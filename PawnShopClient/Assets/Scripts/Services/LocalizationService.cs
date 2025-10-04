using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Repositories;
using UnityEngine;

namespace PawnShop.Services
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ILanguageRepository _languageRepository;
        private Dictionary<string, string[]> _localizationDictionary;
        private Language _currentLanguage;

        public event System.Action OnLocalizationSwitch;

        public LocalizationService(ILanguageRepository languageRepository)
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
                    return $"[{key}]";
                }

                // If only one value, return it directly
                if (values.Length == 1)
                {
                    return values[0];
                }

                // If multiple values, return a random one
                var selectedValue = values[Random.Range(0, values.Length)];
                return selectedValue;
            }

            Debug.LogWarning($"Localization key '{key}' not found for language {_currentLanguage}");
            return $"[{key}]";
        }

        private void LoadLocalizationFile(string fileName)
        {
            Debug.Log($"[LocalizationService] Loading localization file: {fileName}");
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
                Debug.Log($"[LocalizationService] JSON content length: {jsonContent.Length}");

                var localizationData = JsonUtility.FromJson<LocalizationData>(jsonContent);
                Debug.Log($"[LocalizationService] Parsed {localizationData?.entries?.Length ?? 0} entries from JSON");

                foreach (var entry in localizationData.entries)
                {
                    _localizationDictionary[entry.key] = entry.values;
                    Debug.Log($"[LocalizationService] Loaded entry: {entry.key} = {entry.values?.Length ?? 0} values");
                }

                Debug.Log($"Loaded {_localizationDictionary.Count} localization entries from {fileName}");

                // Check if our new keys are loaded
                var greetingKey = "dialog_customer_greeting";
                var buyerKey = "dialog_customer_buyer_intent";
                var sellerKey = "dialog_customer_seller_intent";

                Debug.Log($"[LocalizationService] Greeting key exists: {_localizationDictionary.ContainsKey(greetingKey)}");
                Debug.Log($"[LocalizationService] Buyer key exists: {_localizationDictionary.ContainsKey(buyerKey)}");
                Debug.Log($"[LocalizationService] Seller key exists: {_localizationDictionary.ContainsKey(sellerKey)}");
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
}
