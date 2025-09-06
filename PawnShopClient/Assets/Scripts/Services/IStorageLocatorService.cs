using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IStorageLocatorService
    {
        ISlotStorageService<ItemModel> Get(StorageType type);
    }
}