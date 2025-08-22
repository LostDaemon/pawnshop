public interface ILanguageRepository
{
    LanguagePrototype GetLanguage(Language language);
    void Load();
}
