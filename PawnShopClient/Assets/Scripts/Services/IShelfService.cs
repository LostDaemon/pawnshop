using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IShelfService
    {
        void RegisterMapping(string slotId, string itemId);
        void UnregisterMapping(string slotId);
        bool HasMapping(string slotId);
        string GetItemId(string slotId);
        void ClearAllMappings();
    }
}
