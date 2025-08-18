using System;

public interface INegotiationService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action<long> OnCurrentOfferChanged;
    event Action OnSkipRequested;

    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }
    long GetCurrentOffer();

    void SetCurrentCustomer(Customer customer);
    bool MakeDiscountOffer(float discount);
    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
    void RequestSkip();
    void AskAboutItemOrigin();
    void AnalyzeItem();
}