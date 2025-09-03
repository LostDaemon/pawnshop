using System;
using System.Linq;
using PawnShop.Models;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class ItemTransferService : IItemTransferService
    {
        private readonly IStorageLocatorService _storageLocator;
        private readonly IStorageRouterService<ItemModel> _storageRouter;

        public event Action<ItemModel, StorageType, StorageType> OnItemTransferCompleted;

        [Inject]
        public ItemTransferService(
            IStorageLocatorService storageLocator,
            IStorageRouterService<ItemModel> storageRouter)
        {
            _storageLocator = storageLocator;
            _storageRouter = storageRouter;
        }

        public bool CanTransferItem(ItemModel item, StorageType sourceType, StorageType targetType)
        {
            if (item == null) return false;
            if (sourceType == targetType) return false;

            var sourceStorage = _storageLocator.Get(sourceType);
            var targetStorage = _storageLocator.Get(targetType);

            if (sourceStorage == null || targetStorage == null) return false;

            // Check if item exists in source storage
            if (!sourceStorage.All.Contains(item)) return false;

            // Check if target storage can accept the item
            // This could include capacity checks, type restrictions, etc.
            return true;
        }

        public bool TransferItem(ItemModel item, StorageType sourceType, StorageType targetType)
        {
            if (!CanTransferItem(item, sourceType, targetType))
            {
                Debug.LogWarning($"Cannot transfer item {item.Name} from {sourceType} to {targetType}");
                return false;
            }

            var sourceStorage = _storageLocator.Get(sourceType);
            var targetStorage = _storageLocator.Get(targetType);

            try
            {
                // Use storage router for the transfer
                _storageRouter.Transfer(item, sourceStorage, targetStorage);

                // Invoke the event
                OnItemTransferCompleted?.Invoke(item, sourceType, targetType);

                Debug.Log($"Successfully transferred item {item.Name} from {sourceType} to {targetType}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to transfer item {item.Name}: {e.Message}");
                return false;
            }
        }
    }
}
