public interface IStorageLocatorService
{
    IGameStorageService<ItemModel> Get(StorageType type);
}