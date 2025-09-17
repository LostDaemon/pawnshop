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
        // Analysis delay constants (in game ticks)
        private const int VISUAL_ANALYSIS_DELAY = 1;
        private const int DEFECTOSCOPE_DELAY = 3;
        private const int HISTORY_RESEARCH_DELAY = 2;
        private const int PURITY_ANALYZER_DELAY = 3;
        private const int DOCUMENT_INSPECTION_DELAY = 2;
        private const int LEGAL_STATUS_DELAY = 2;
        private const int DEFAULT_ANALYSIS_DELAY = 1;

        private readonly IWalletService _wallet;
        private readonly ISlotStorageService<ItemModel> _inventoryStorage;
        private readonly ISlotStorageService<ItemModel> _sellStorage;
        private readonly ICustomerService _customerService;
        private readonly INegotiationHistoryService _history;
        private readonly ILocalizationService _localizationService;
        private readonly IItemInspectionService _inspectionService;
        private readonly IPlayerService _playerService;
        private readonly IEvaluationService _evaluationService;
        private readonly ITimeService _timeService;

        // Analysis delay tracking
        private AnalyzeType? _pendingAnalysisType;
        private int _remainingDelayTicks;

        public event Action OnDealSuccess;
        public event Action OnNegotiationStarted;
        public event Action<ItemModel> OnCurrentOfferChanged;
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
            IEvaluationService evaluationService,
            ITimeService timeService)
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
            _timeService = timeService;

            // Subscribe to events
            _customerService.OnCustomerChanged += OnCustomerChanged;
            _timeService.OnTimeChanged += OnTimeChanged;
        }

        public long GetCurrentOffer() => CurrentItem?.CurrentOffer ?? 0;

        private void OnCustomerChanged(Customer customer)
        {
            Debug.Log($"[NegotiationService] Customer changed to: {customer?.CustomerType}");

            // Clear current negotiation state when customer changes
            if (customer != null)
            {
                StartNegotiation(customer);
            }
        }

        private void OnTimeChanged(GameTime currentTime)
        {
            // Process pending analysis if delay is complete
            if (_pendingAnalysisType.HasValue && _remainingDelayTicks > 0)
            {
                _remainingDelayTicks--;
                Debug.Log($"[NegotiationService] Analysis delay: {_remainingDelayTicks} ticks remaining for {_pendingAnalysisType.Value}");
                
                if (_remainingDelayTicks <= 0)
                {
                    // Delay complete, perform analysis
                    PerformAnalysis(_pendingAnalysisType.Value);
                    _pendingAnalysisType = null;
                }
            }
        }

        private void StartNegotiation(Customer customer)
        {
            Debug.Log($"[NegotiationService] CurrentCustomer: {CurrentCustomer?.CustomerType}, CurrentItem: {CurrentItem?.Name}, You paid {CurrentItem?.PurchasePrice}");

            // Let customer inspect the item to reveal tags based on their skills before evaluation
            _inspectionService.InspectByCustomer(CurrentItem);

            GenerateInitialNpcOffer();

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
                var sellerMessage = string.Format(_localizationService.GetLocalization("dialog_customer_seller_intent"), CurrentItem.Name, CurrentItem.CurrentOffer);
                Debug.Log($"[NegotiationService] Adding seller intent: {sellerMessage}");
                _history.Add(new TextRecord(HistoryRecordSource.Customer, sellerMessage));
            }

            OnNegotiationStarted?.Invoke();
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




        public List<BaseTagModel> AskAboutItemOrigin()
        {
            if (CurrentItem == null || CurrentCustomer == null)
                return new List<BaseTagModel>();

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
                return new List<BaseTagModel>();
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

            return customerKnownTags;
        }

        private void HandleSellerKnowledge(List<BaseTagModel> customerKnownTags)
        {
            Debug.Log($"[NegotiationService] HandleSellerKnowledge called with {customerKnownTags?.Count ?? 0} known tags");
            
            if (customerKnownTags == null)
            {
                Debug.LogWarning("[NegotiationService] HandleSellerKnowledge: customerKnownTags is null");
                return;
            }

            // Log all known tags for debugging
            Debug.Log($"[NegotiationService] Customer known tags:");
            for (int i = 0; i < customerKnownTags.Count; i++)
            {
                var tag = customerKnownTags[i];
                string tagName = tag?.DisplayName ?? "Unknown";
                float multiplier = tag?.PriceMultiplier ?? 0f;
                Debug.Log($"  [{i}] {tagName} - Multiplier: {multiplier}");
            }

            // Get positive tags (tags with positive price multiplier)
            var positiveTags = customerKnownTags.Where(t => t.PriceMultiplier > 1.0f).ToList();
            Debug.Log($"[NegotiationService] Found {positiveTags.Count} positive tags (multiplier > 1.0)");

            if (positiveTags.Count == 0)
            {
                Debug.Log("[NegotiationService] No positive tags found - customer knows nothing about positive aspects");
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
                Debug.Log($"[NegotiationService] Formatting positive tag: {tag.DisplayName} (multiplier: {tag.PriceMultiplier})");
                formattedTags += TagTextRenderHelper.RenderTag(tag);

                if (i < positiveTags.Count - 1)
                {
                    formattedTags += " ";
                }
            }

            Debug.Log($"[NegotiationService] Formatted tags string: {formattedTags}");

            // Add customer's knowledge to history
            string customerKnowledgeMessage = string.Format(
                _localizationService.GetLocalization("dialog_customer_seller_knows_item_positive"),
                formattedTags);

            Debug.Log($"[NegotiationService] Adding customer knowledge message to history: {customerKnowledgeMessage}");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, customerKnowledgeMessage));
            
            // Make revealed positive tags known to player
            foreach (var tag in positiveTags)
            {
                if (!tag.IsRevealedToPlayer)
                {
                    tag.IsRevealedToPlayer = true;
                }
            }
            OnTagsRevealed?.Invoke(CurrentItem);
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
                
                // Reduce customer patience by 10 for rejected counter offer
                _customerService.ChangeCustomerPatience(-10f);
            }

            return accepted;
        }

        public void AnalyzeItem(AnalyzeType analyzeType = AnalyzeType.Undefined)
        {
            // Check if analysis is already in progress
            if (_pendingAnalysisType.HasValue)
            {
                Debug.Log($"[NegotiationService] Analysis already in progress for {_pendingAnalysisType.Value}, ignoring new request");
                return;
            }

            // Start delayed analysis
            int delayTicks = GetAnalysisDelay(analyzeType);
            _pendingAnalysisType = analyzeType;
            _remainingDelayTicks = delayTicks;
            
            Debug.Log($"[NegotiationService] Starting analysis delay of {delayTicks} ticks for {analyzeType}");
        }

        private void PerformAnalysis(AnalyzeType analyzeType)
        {
            Debug.Log($"[NegotiationService] Performing analysis: {analyzeType}");
            _inspectionService.InspectByPlayer(CurrentItem, analyzeType);
            OnTagsRevealed?.Invoke(CurrentItem);
        }

        private int GetAnalysisDelay(AnalyzeType analyzeType)
        {
            return analyzeType switch
            {
                AnalyzeType.VisualAnalysis => VISUAL_ANALYSIS_DELAY,
                AnalyzeType.Defectoscope => DEFECTOSCOPE_DELAY,
                AnalyzeType.HistoryResearch => HISTORY_RESEARCH_DELAY,
                AnalyzeType.PurityAnalyzer => PURITY_ANALYZER_DELAY,
                AnalyzeType.DocumentInspection => DOCUMENT_INSPECTION_DELAY,
                AnalyzeType.CheckLegalStatus => LEGAL_STATUS_DELAY,
                _ => DEFAULT_ANALYSIS_DELAY
            };
        }

        public void DeclareTags(List<BaseTagModel> tags, long offerPrice)
        {
            if (tags == null || tags.Count == 0)
                return;

            // Format tags for display
            string formattedTags = "";
            for (int i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                formattedTags += TagTextRenderHelper.RenderTag(tag);
                if (i < tags.Count - 1)
                {
                    formattedTags += " ";
                }
            }

            // Add player's declaration to history
            string playerMessage = string.Format(_localizationService.GetLocalization("dialog_player_declares_tags"), formattedTags, offerPrice);
            _history.Add(new TextRecord(HistoryRecordSource.Player, playerMessage));

            // Make tags known to customer
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

        public void Dispose()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
            }
            if (_timeService != null)
            {
                _timeService.OnTimeChanged -= OnTimeChanged;
            }
        }
    }
}