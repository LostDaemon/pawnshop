using System;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IItemTransferService
    {
        event Action<ItemModel, StorageType, StorageType> OnItemTransferCompleted;
        bool CanTransferItem(ItemModel item, StorageType sourceType, StorageType targetType);
        bool TransferItem(ItemModel item, StorageType sourceType, StorageType targetType);
    }
}
