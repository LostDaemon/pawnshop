using System.Collections.Generic;
using Zenject;

public static class GameStorageMapper
{
    private static Dictionary<StorageType, IGameStorageService<ItemModel>> _storageMap = new()
    {
       {StorageType.InventoryStorage, ProjectContext.Instance.Container.ResolveId<IGameStorageService<ItemModel>>(StorageType.InventoryStorage)},
       {StorageType.SellStorage, ProjectContext.Instance.Container.ResolveId<IGameStorageService<ItemModel>>(StorageType.SellStorage)},
    };

    public static IGameStorageService<ItemModel> GetStorage(StorageType storageType)
    {
        if (_storageMap.TryGetValue(storageType, out var storage))
        {
            return storage;
        }

        throw new KeyNotFoundException($"Storage type {storageType} not found in GameStorageMapper.");
    }
}