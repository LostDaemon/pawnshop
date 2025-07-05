using System;
using System.Collections.Generic;
using Zenject;

public class StorageLocatorService : IStorageLocatorService
{
    private readonly Dictionary<StorageType, IGameStorageService<ItemModel>> _map = new();

    [Inject]
    public void Construct(
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        [Inject(Id = StorageType.SellStorage)] IGameStorageService<ItemModel> sell)
    {
        _map[StorageType.InventoryStorage] = inventory;
        _map[StorageType.SellStorage] = sell;
    }

    public IGameStorageService<ItemModel> Get(StorageType type)
    {
        if (_map.TryGetValue(type, out var storage))
            return storage;

        throw new NotSupportedException($"Storage type {type} not found in locator service.");
    }
}