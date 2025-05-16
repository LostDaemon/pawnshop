using System;

public interface INegotiateService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;

    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }

    void SetCurrentItem(ItemModel item);
    void RequestSkip();

    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
}