using System;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public interface INegotiationService
    {
        event Action OnDealSuccess;
        event Action OnNegotiationStarted;
        event Action<ItemModel> OnCurrentOfferChanged;
        event Action<ItemModel> OnTagsRevealed;
        ItemModel CurrentItem { get; }
        long GetCurrentOffer();
        bool MakeCounterOffer(long newOffer);
        bool TryMakeDeal(long offeredPrice);
        List<BaseTagModel> AskAboutItemOrigin();
        void DeclareTags(List<BaseTagModel> tags, long offerPrice);

        /// <summary>
        /// Analyze the current item with specific analysis type
        /// </summary>
        void AnalyzeItem(AnalyzeType analyzeType = AnalyzeType.Undefined);
    }
}