using System;
using System.Collections.Generic;

public interface INegotiationService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;

    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }
    long GetCurrentOffer();

    void SetCurrentCustomer(Customer customer);
    bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock);
    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
    void RequestSkip();
    void AskAboutItemOrigin();
}