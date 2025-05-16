using System;

public interface IPurchaseService
{
    void SetCurrentItem(ItemModel item);
    bool TryPurchase(long offeredPrice);
    void RequestSkip();
    ItemModel CurrentItem { get; }
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;
}