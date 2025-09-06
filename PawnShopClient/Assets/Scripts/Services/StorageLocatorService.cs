using System;
using System.Collections.Generic;
using PawnShop.Models;
using Zenject;

namespace PawnShop.Services
{
    public class StorageLocatorService : IStorageLocatorService
    {
        private readonly Dictionary<StorageType, ISlotStorageService<ItemModel>> _map = new();

        [Inject]
        public void Construct(
            [Inject(Id = StorageType.InventoryStorage)] ISlotStorageService<ItemModel> inventory,
            [Inject(Id = StorageType.SellStorage)] ISlotStorageService<ItemModel> sell)
        {
            _map[StorageType.InventoryStorage] = inventory;
            _map[StorageType.SellStorage] = sell;
        }

        public ISlotStorageService<ItemModel> Get(StorageType type)
        {
            if (_map.TryGetValue(type, out var storage))
                return storage;

            throw new NotSupportedException($"Storage type {type} not found in locator service.");
        }
    }
}