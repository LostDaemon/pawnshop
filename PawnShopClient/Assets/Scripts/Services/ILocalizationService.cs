using PawnShop.Models;

namespace PawnShop.Services
{
    public interface ILocalizationService
    {
        void SwitchLocalization(Language language);
        string GetLocalization(string key);
        event System.Action OnLocalizationSwitch;
    }
}
