using System;
using System.Collections.Generic;

public interface INegotiateService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;

    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }

    void SetCurrentItem(ItemModel item);
    bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock);
    bool TryPurchase(long offeredPrice);
    void RequestSkip();
    long GetCurrentOffer();
}