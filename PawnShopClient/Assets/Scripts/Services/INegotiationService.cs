using System;
using System.Collections.Generic;

public interface INegotiationService
{
    event Action<ItemModel> OnPurchased;
    event Action<ItemModel> OnCurrentItemChanged;
    event Action<long> OnCurrentOfferChanged;
    event Action OnSkipRequested;
    event Action OnTagsRevealed;

    ItemModel CurrentItem { get; }
    long CurrentNpcOffer { get; }
    long GetCurrentOffer();

    void SetCurrentCustomer(Customer customer);
    bool MakeDiscountOffer(float discount);
    bool TryPurchase(long offeredPrice);
    bool TryCounterOffer(long playerOffer);
    void RequestSkip();
    void AskAboutItemOrigin();

    /// <summary>
    /// Get visible tags for the current item (filtered by IsRevealed)
    /// </summary>
    /// <returns>List of visible tags</returns>
    List<BaseTagModel> GetVisibleTags();

    /// <summary>
    /// Check if a tag should be visible to the player
    /// </summary>
    /// <param name="tag">Tag to check</param>
    /// <returns>True if tag should be visible</returns>
    bool IsTagVisible(BaseTagModel tag);

    /// <summary>
    /// Reveal a hidden tag (make it visible)
    /// </summary>
    /// <param name="tag">Tag to reveal</param>
    void RevealTag(BaseTagModel tag);

    /// <summary>
    /// Analyze the current item (placeholder for future complex logic)
    /// </summary>
    void AnalyzeItem();
}