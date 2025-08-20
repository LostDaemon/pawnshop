using System;
using System.Collections.Generic;

public interface INegotiationService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action<long> OnCurrentOfferChanged;
    event Action OnSkipRequested;
    event Action<List<BaseTagModel>> OnTagsRevealed;
    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }
    long GetCurrentOffer();
    bool MakeDiscountOffer(float discount);
    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
    void RequestSkip();
    void AskAboutItemOrigin();
    void ShowNextCustomer();

    /// <summary>
    /// Analyze the current item (placeholder for future complex logic)
    /// </summary>
    void AnalyzeItem();
}