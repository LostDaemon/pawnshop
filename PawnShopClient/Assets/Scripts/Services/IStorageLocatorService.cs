using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IStorageLocatorService
    {
        IGameStorageService<ItemModel> Get(StorageType type);
    }
}