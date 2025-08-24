using System;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Models.Tags;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class NegotiationService : INegotiationService
    {
        private readonly IWalletService _wallet;
        private readonly IGameStorageService<ItemModel> _inventory;
        private readonly ICustomerService _customerService;
        private readonly INegotiationHistoryService _history;
        private readonly ILocalizationService _localizationService;
        private readonly IItemInspectionService _inspectionService;
        private readonly IPlayerService _playerService;
        private readonly IEvaluationService _evaluationService;

        public event Action<ItemModel> OnPurchased;
        public event Action<ItemModel> OnCurrentItemChanged;
        public event Action<ItemModel> OnCurrentOfferChanged;
        public event Action OnSkipRequested;
        public event Action<ItemModel> OnTagsRevealed;

        public ItemModel CurrentItem => _customerService.CurrentCustomer?.OwnedItem;
        public Customer CurrentCustomer => _customerService.CurrentCustomer;


        [Inject]
        public NegotiationService(
            IWalletService wallet,
            [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
            ICustomerService customerService,
            INegotiationHistoryService history,
            ILocalizationService localizationService,
            IItemInspectionService inspectionService,
            IPlayerService playerService,
            IEvaluationService evaluationService)
        {
            _wallet = wallet;
            _inventory = inventory;
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

            Debug.Log($"[NegotiationService] CurrentCustomer: {CurrentCustomer?.CustomerType}, CurrentItem: {CurrentItem?.Name}");

            // Check if customer and item are available
            if (CurrentCustomer == null || CurrentItem == null)
            {
                Debug.LogWarning("[NegotiationService] Customer or item is null in ShowNextCustomer");
                return;
            }

            GenerateInitialNpcOffer();
            OnCurrentItemChanged?.Invoke(CurrentItem);
            // Add customer greeting
            var greetingMessage = _localizationService.GetLocalization("dialog_customer_greeting");
            Debug.Log($"[NegotiationService] Adding greeting: {greetingMessage}");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, greetingMessage));

            // Add customer intent message based on type
            if (CurrentCustomer.CustomerType == CustomerType.Buyer)
            {
                var buyerMessage = string.Format(_localizationService.GetLocalization("dialog_customer_buyer_intent"), CurrentItem.CurrentOffer);
                Debug.Log($"[NegotiationService] Adding buyer intent: {buyerMessage}");
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
            // Use evaluation service to get customer's initial offer based on revealed tags
            var tags = _inspectionService.InspectByCustomer(CurrentItem);
            var positiveTags = tags.Where(tag => tag.IsRevealedToCustomer && tag.PriceMultiplier > 1).ToList();


            Debug.Log($"Customer knows {tags.Count} tags: {string.Join(", ", tags.Select(t => $"{t.DisplayName}({t.PriceMultiplier:F2}x)"))}");
            Debug.Log($"Item has {positiveTags.Count} positive tags: {string.Join(", ", positiveTags.Select(t => $"{t.DisplayName}({t.PriceMultiplier:F2}x)"))}");

            CurrentItem.CurrentOffer = _evaluationService.Evaluate(CurrentItem, positiveTags, EvaluationStrategy.Optimistic); //First offer is based on Customer's overestimation
            Debug.Log($"Generated initial NPC offer: {CurrentItem.CurrentOffer} for item: {CurrentItem.Name}");
        }

        public bool TryPurchase(long offeredPrice)
        {
            if (CurrentItem == null)
                return false;

            var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
            if (!success)
            {
                _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_cant_pay")));
                return false;
            }

            CurrentItem.PurchasePrice = offeredPrice;
            _inventory.Put(CurrentItem);

            CurrentItem.CurrentOffer = offeredPrice;

            _history.Add(new TextRecord(HistoryRecordSource.Customer,
                string.Format(_localizationService.GetLocalization("dialog_customer_deal_accepted"), offeredPrice)));
            OnPurchased?.Invoke(CurrentItem);
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

            if (CurrentItem.IsFake)
            {
                _customerService.IncreaseUncertainty(0.25f);
                _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_uncertain_origin")));
            }
            else
            {
                _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_family_item")));
            }
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
                _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_counter_rejected")));
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
            // Make decision based on offer vs adjusted value
            Debug.Log($"Player offer: {offer}, Adjusted value: {adjustedValue}, Calculated item value: {itemValue}");

            return offer >= adjustedValue;
        }
    }
}