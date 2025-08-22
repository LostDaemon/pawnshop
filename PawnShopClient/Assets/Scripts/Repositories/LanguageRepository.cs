using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.ScriptableObjects;
using UnityEngine;

namespace PawnShop.Repositories
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly List<LanguagePrototype> _languages;

        public LanguageRepository()
        {
            _languages = new List<LanguagePrototype>();
        }

        public void Load()
        {
            _languages.Clear();
            _languages.AddRange(Resources.LoadAll<LanguagePrototype>(@"ScriptableObjects\L10n").ToList());
            Debug.Log($"Loaded {_languages.Count} language prototypes.");
        }

        public LanguagePrototype GetLanguage(Language language)
        {
            return _languages.FirstOrDefault(lang => lang.Language == language);
        }
    }
}
