using PawnShop.Models;
using PawnShop.ScriptableObjects;

namespace PawnShop.Repositories
{
    public interface ILanguageRepository
    {
        LanguagePrototype GetLanguage(Language language);
        void Load();
    }
}
