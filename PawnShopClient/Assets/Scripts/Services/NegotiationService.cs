using System;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Models.Tags;
using PawnShop.Helpers;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class NegotiationService : INegotiationService
    {
        private readonly IWalletService _wallet;
        private readonly ISlotStorageService<ItemModel> _inventoryStorage;
        private readonly ISlotStorageService<ItemModel> _sellStorage;
        private readonly ICustomerService _customerService;
        private readonly INegotiationHistoryService _history;
        private readonly ILocalizationService _localizationService;
        private readonly IItemInspectionService _inspectionService;
        private readonly IPlayerService _playerService;
        private readonly IEvaluationService _evaluationService;

        public event Action OnDealSuccess;
        public event Action<ItemModel> OnCurrentItemChanged;
        public event Action<ItemModel> OnCurrentOfferChanged;
        public event Action OnSkipRequested;
        public event Action<ItemModel> OnTagsRevealed;

        public ItemModel CurrentItem => _customerService.CurrentCustomer?.OwnedItem;
        public Customer CurrentCustomer => _customerService.CurrentCustomer;


        [Inject]
        public NegotiationService(
            IWalletService wallet,
            [Inject(Id = StorageType.InventoryStorage)] ISlotStorageService<ItemModel> inventoryStorage,
            [Inject(Id = StorageType.SellStorage)] ISlotStorageService<ItemModel> sellStorage,
            ICustomerService customerService,
            INegotiationHistoryService history,
            ILocalizationService localizationService,
            IItemInspectionService inspectionService,
            IPlayerService playerService,
            IEvaluationService evaluationService)
        {
            _wallet = wallet;
            _inventoryStorage = inventoryStorage;
            _sellStorage = sellStorage;
            _customerService = customerService;
            _history = history;
            _localizationService = localizationService;
            _inspectionService = inspectionService;
            _playerService = playerService;
            _evaluationService = evaluationService;
        }

        public long GetCurrentOffer() => CurrentItem?.CurrentOffer ?? 0;

        public void ShowNextCustomer()
        {
            Debug.Log("[NegotiationService] ShowNextCustomer called");
            _customerService.ShowNextCustomer();

            Debug.Log($"[NegotiationService] CurrentCustomer: {CurrentCustomer?.CustomerType}, CurrentItem: {CurrentItem?.Name}, You paid {CurrentItem?.PurchasePrice}");

            // Check if customer and item are available
            if (CurrentCustomer == null || CurrentItem == null)
            {
                Debug.LogWarning($"[NegotiationService] Customer or item is null in ShowNextCustomer: You paid {CurrentItem?.PurchasePrice}");
                return;
            }

            // Let customer inspect the item to reveal tags based on their skills before evaluation
            _inspectionService.InspectByCustomer(CurrentItem);

            GenerateInitialNpcOffer();
            OnCurrentItemChanged?.Invoke(CurrentItem);
            // Add customer greeting
            var greetingMessage = _localizationService.GetLocalization("dialog_customer_greeting");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, greetingMessage));

            // Add customer intent message based on type
            if (CurrentCustomer.CustomerType == CustomerType.Buyer)
            {
                var buyerMessage = string.Format(_localizationService.GetLocalization("dialog_customer_buyer_intent"), CurrentItem.CurrentOffer);
                _history.Add(new TextRecord(HistoryRecordSource.Customer, buyerMessage));
            }
            else
            {
                var sellerMessage = string.Format(_localizationService.GetLocalization("dialog_customer_seller_intent"), CurrentItem.CurrentOffer);
                Debug.Log($"[NegotiationService] Adding seller intent: {sellerMessage}");
                _history.Add(new TextRecord(HistoryRecordSource.Customer, sellerMessage));
            }
        }

        private void GenerateInitialNpcOffer()
        {
            if (CurrentItem == null) return;

            // Use different evaluation strategies based on customer type
            var strategy = CurrentCustomer.CustomerType == CustomerType.Seller
                ? EvaluationStrategy.Optimistic  // Seller overestimates item value
                : EvaluationStrategy.Pessimistic; // Buyer underestimates item value

            CurrentItem.CurrentOffer = _evaluationService.EvaluateByCustomer(CurrentItem, strategy);
        }

        public bool TryMakeDeal(long offeredPrice)
        {
            if (CurrentItem == null)
                return false;

            if (CurrentCustomer.CustomerType == CustomerType.Seller)
            {
                // Seller logic: player buys item from customer
                var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
                if (!success)
                {
                    _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_cant_pay")));
                    return false;
                }

                // Reset all customer-revealed tags when player buys the item
                // This ensures new customers don't know what previous customers knew
                foreach (var tag in CurrentItem.Tags)
                {
                    tag.IsRevealedToCustomer = false;
                }

                CurrentItem.PurchasePrice = offeredPrice;
                if (!_inventoryStorage.Put(CurrentItem))
                {
                    _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_inventory_full")));
                    return false;
                }
            }
            else
            {
                // Buyer logic: player sells item to customer
                var success = _wallet.TransactionAttempt(CurrencyType.Money, offeredPrice);
                if (!success)
                {
                    _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_deal_joy")));
                    return false;
                }

                // Set the sell price for the item being sold
                CurrentItem.SellPrice = offeredPrice;

                // Remove item from sellstorage permanently
                _sellStorage.Withdraw(CurrentItem);
            }

            CurrentItem.CurrentOffer = offeredPrice;

            _history.Add(new TextRecord(HistoryRecordSource.Customer,
                string.Format(_localizationService.GetLocalization("dialog_customer_deal_accepted"), offeredPrice)));
            OnDealSuccess?.Invoke();
            return true;
        }



        public void RequestSkip()
        {
            _history.Add(new TextRecord(HistoryRecordSource.Player,
                string.Format(_localizationService.GetLocalization("dialog_player_skip_item"), CurrentItem?.Name)));
            OnSkipRequested?.Invoke();
        }

        public void AskAboutItemOrigin()
        {
            if (CurrentItem == null || CurrentCustomer == null)
                return;

            // Get customer's knowledge about the item
            var customerKnownTags = CurrentItem.Tags.Where(t => t.IsRevealedToCustomer).ToList();

            if (customerKnownTags.Count == 0)
            {
                // Customer doesn't know anything about the item
                string noKnowledgeKey = CurrentCustomer.CustomerType == CustomerType.Seller
                    ? "dialog_customer_seller_knows_nothing"
                    : "dialog_customer_buyer_knows_no_negative";

                if (CurrentCustomer.CustomerType == CustomerType.Seller)
                {
                    _history.Add(new TextRecord(HistoryRecordSource.Customer,
                        _localizationService.GetLocalization(noKnowledgeKey)));
                }
                else
                {
                    // For buyer, show their offer when they know nothing
                    string neutralMessage = string.Format(
                        _localizationService.GetLocalization(noKnowledgeKey),
                        CurrentItem.CurrentOffer);
                    _history.Add(new TextRecord(HistoryRecordSource.Customer, neutralMessage));
                }
                return;
            }

            if (CurrentCustomer.CustomerType == CustomerType.Seller)
            {
                // Seller logic: show positive tags they know about
                HandleSellerKnowledge(customerKnownTags);
            }
            else
            {
                // Buyer logic: show negative tags they see and justify their offer
                HandleBuyerKnowledge(customerKnownTags);
            }
        }

        private void HandleSellerKnowledge(List<BaseTagModel> customerKnownTags)
        {
            // Get positive tags (tags with positive price multiplier)
            var positiveTags = customerKnownTags.Where(t => t.PriceMultiplier > 1.0f).ToList();

            if (positiveTags.Count == 0)
            {
                // Customer knows about the item but no positive aspects - treat as knowing nothing
                _history.Add(new TextRecord(HistoryRecordSource.Customer,
                    _localizationService.GetLocalization("dialog_customer_seller_knows_nothing")));
                return;
            }

            // Format positive tags with colors using helper
            string formattedTags = "";
            for (int i = 0; i < positiveTags.Count; i++)
            {
                var tag = positiveTags[i];
                formattedTags += TagTextRenderHelper.RenderTag(tag);

                if (i < positiveTags.Count - 1)
                {
                    formattedTags += " ";
                }
            }

            // Add customer's knowledge to history
            string customerKnowledgeMessage = string.Format(
                _localizationService.GetLocalization("dialog_customer_seller_knows_item_positive"),
                formattedTags);

            _history.Add(new TextRecord(HistoryRecordSource.Customer, customerKnowledgeMessage));
        }

        private void HandleBuyerKnowledge(List<BaseTagModel> customerKnownTags)
        {
            // Get negative tags (tags with negative price multiplier)
            var negativeTags = customerKnownTags.Where(t => t.PriceMultiplier < 1.0f).ToList();

            if (negativeTags.Count == 0)
            {
                // Buyer sees no negative aspects - neutral positive response
                string neutralMessage = string.Format(
                    _localizationService.GetLocalization("dialog_customer_buyer_knows_no_negative"),
                    CurrentItem.CurrentOffer);
                _history.Add(new TextRecord(HistoryRecordSource.Customer, neutralMessage));
                return;
            }

            // Format negative tags with colors using helper
            string formattedTags = "";
            for (int i = 0; i < negativeTags.Count; i++)
            {
                var tag = negativeTags[i];
                formattedTags += TagTextRenderHelper.RenderTag(tag);

                if (i < negativeTags.Count - 1)
                {
                    formattedTags += " ";
                }
            }

            // Add buyer's justification for their offer based on negative tags
            string buyerMessage = string.Format(
                _localizationService.GetLocalization("dialog_customer_buyer_knows_negative"),
                formattedTags, CurrentItem.CurrentOffer);

            _history.Add(new TextRecord(HistoryRecordSource.Customer, buyerMessage));
        }

        public bool MakeCounterOffer(long newOffer)
        {
            _history.Add(new TextRecord(HistoryRecordSource.Player,
                string.Format(_localizationService.GetLocalization("dialog_player_counter_offer"), newOffer)));

            var accepted = ProcessPlayerOffer(newOffer);

            if (accepted)
            {
                CurrentItem.CurrentOffer = newOffer;
                Debug.Log($"Counter offer accepted: {newOffer}");
                _history.Add(new TextRecord(HistoryRecordSource.Customer,
                    string.Format(_localizationService.GetLocalization("dialog_customer_counter_accepted"), newOffer)));
                OnCurrentOfferChanged?.Invoke(CurrentItem);
            }
            else
            {
                Debug.Log("Counter offer rejected.");
                // Use different rejection messages based on customer type
                string rejectionKey = CurrentCustomer.CustomerType == CustomerType.Seller
                    ? "dialog_customer_seller_counter_rejected"
                    : "dialog_customer_buyer_counter_rejected";
                _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization(rejectionKey)));
            }

            return accepted;
        }

        public void AnalyzeItem()
        {
            var revealedTags = _inspectionService.InspectByPlayer(CurrentItem);

            // Add to history
            _history.Add(new TextRecord(HistoryRecordSource.Player,
            string.Format(_localizationService.GetLocalization("dialog_player_analyzed_item"), CurrentItem.Name, revealedTags.Count)));
            // Notify that tags were revealed
            OnTagsRevealed?.Invoke(CurrentItem);
        }

        public void DeclareTags(List<BaseTagModel> tags)
        {
            foreach (var tag in tags)
            {
                var curTag = CurrentItem.Tags.FirstOrDefault(t => t.ClassId == tag.ClassId);
                if (curTag != null)
                {
                    curTag.IsRevealedToCustomer = true;
                }
            }
        }

        private bool ProcessPlayerOffer(long offer)
        {
            long itemValue = _evaluationService.EvaluateByCustomer(CurrentItem, EvaluationStrategy.Realistic);
            const float deviationRange = 0.1f; // Add random deviation ±10%
            float deviation = UnityEngine.Random.Range(-deviationRange, deviationRange);
            long adjustedValue = (long)(itemValue * (1 + deviation));

            // Different acceptance logic based on customer type
            if (CurrentCustomer.CustomerType == CustomerType.Seller)
            {
                return offer >= adjustedValue; // Seller accepts if offer is at least realistic value
            }
            else
            {
                return offer <= adjustedValue; // Buyer accepts if offer is at most realistic value
            }
        }
    }
}