using System;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public interface INegotiationService
    {
        event Action<ItemModel> OnPurchased;
        event Action<ItemModel> OnCurrentItemChanged;
        event Action<ItemModel> OnCurrentOfferChanged;
        event Action OnSkipRequested;
        event Action<ItemModel> OnTagsRevealed;
        ItemModel CurrentItem { get; }
        long GetCurrentOffer();
        bool MakeCounterOffer(long newOffer);
        bool TryPurchase(long offeredPrice);
        void RequestSkip();
        void AskAboutItemOrigin();
        void ShowNextCustomer();
        void DeclareTags(List<BaseTagModel> tags);

        /// <summary>
        /// Analyze the current item (placeholder for future complex logic)
        /// </summary>
        void AnalyzeItem();
    }
}