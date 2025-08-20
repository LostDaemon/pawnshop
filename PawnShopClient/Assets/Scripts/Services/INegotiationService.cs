using System;
using System.Collections.Generic;

public interface INegotiationService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action<ItemModel> OnCurrentOfferChanged;
    event Action OnSkipRequested;
    event Action<ItemModel> OnTagsRevealed;
    ItemModel CurrentItem { get; }
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