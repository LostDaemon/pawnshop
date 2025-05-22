using System;
using System.Collections.Generic;

public interface INegotiateService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;

    ItemModel CurrentItem { get; }
    Customer CurrentCustomer { get; }
    long CurrentNpcOffer { get; }

    long GetCurrentOffer();
    void SetCurrentCustomer(Customer customer);
    bool TryPurchase(long offeredPrice);
    bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock);
    bool TryCounterOffer(long playerOffer);
    void AskAboutItemOrigin();
    void RequestSkip();


    IReadOnlyList<IHistoryRecord> History { get; }
    event Action<IHistoryRecord> OnRecordAdded;
}