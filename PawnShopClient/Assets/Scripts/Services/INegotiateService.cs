using System;
using System.Collections.Generic;

public interface INegotiateService
{
    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }
    IReadOnlyList<IHistoryRecord> History { get; }

    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action OnSkipRequested;
    event Action<IHistoryRecord> OnRecordAdded;

    void SetCurrentCustomer(Customer customer);

    long GetCurrentOffer();

    bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock);

    bool TryPurchase(long offeredPrice);

    bool TryCounterOffer(long playerOffer);

    void RequestSkip();

    void AskAboutItemOrigin();
}