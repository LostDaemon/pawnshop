using System;
using System.Collections.Generic;

public interface INegotiateService
{
    ItemModel CurrentItem { get; }
    Customer CurrentCustomer { get; }
    long CurrentNpcOffer { get; }

    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;

    void SetCurrentCustomer(Customer customer);
    long GetCurrentOffer();
    bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock);
    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
    void RequestSkip();
}