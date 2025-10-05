using UnityEngine;
using UnityEngine.UI;
using PawnShop.Services;
using Zenject;
using PawnShop.Models.Tags;
using PawnShop.Models.Characters;
using PawnShop.Models;
using System.Collections.Generic;
using System.Collections;
using PawnShop.Controllers.DragNDrop;
using PawnShop.Controllers;

namespace PawnShop.Controllers.Cards
{
    public class CardNegotiationController : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _cardSlotPrefab;
        [SerializeField] private GameObject _negotiationRoundPrefab;
        [SerializeField] private Transform _playerCardContainer;
        [SerializeField] private Transform _customerCardContainer;
        [SerializeField] private Transform _negotiationContainer;
        [SerializeField] private Text _initialPrice;
        [SerializeField] private Text _negotiatedPrice;
        [SerializeField] private Text _paidPrice;

        private ICardNegotiationService _cardNegotiationService;
        private DiContainer _container;
        private ItemInfoController _itemInfoController;
        private Dictionary<int, NegotiationRoundController> _roundControllers = new Dictionary<int, NegotiationRoundController>();

        [Inject]
        public void Construct(ICardNegotiationService cardNegotiationService, DiContainer container, ItemInfoController itemInfoController)
        {
            _cardNegotiationService = cardNegotiationService;
            _container = container;
            _itemInfoController = itemInfoController;

            // Subscribe to service events
            _cardNegotiationService.OnCustomerChanged += OnCustomerChanged;
            _cardNegotiationService.OnPriceChanged += OnPriceChanged;
            _cardNegotiationService.OnRoundChanged += OnRoundChanged;
            _cardNegotiationService.OnCustomerPlayed += OnCustomerPlayed;
            _cardNegotiationService.OnPlayerPlayed += OnPlayerPlayed;
        }

        private void InitializeNegotiationRounds()
        {
            if (_negotiationRoundPrefab == null || _negotiationContainer == null) return;

            // Clear existing rounds first
            ClearExistingRounds();
            _roundControllers.Clear();

            // Get rounds from service
            var rounds = _cardNegotiationService.NegotiationRounds;
            if (rounds == null) return;

            int roundIndex = 0;
            foreach (var round in rounds.Values)
            {
                var roundInstance = _container.InstantiatePrefab(_negotiationRoundPrefab, _negotiationContainer);
                var roundController = roundInstance.GetComponent<NegotiationRoundController>();
                if (roundController != null)
                {
                    // Initialize round number
                    roundController.InitializeRound(roundIndex);

                    // Subscribe to multiplier changes
                    roundController.OnMultiplierChanged += OnRoundMultiplierChanged;

                    // Add to dictionary with zero-based index
                    _roundControllers[roundIndex] = roundController;
                    roundIndex++;
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from service events
            if (_cardNegotiationService != null)
            {
                _cardNegotiationService.OnCustomerChanged -= OnCustomerChanged;
                _cardNegotiationService.OnPriceChanged -= OnPriceChanged;
                _cardNegotiationService.OnRoundChanged -= OnRoundChanged;
                _cardNegotiationService.OnCustomerPlayed -= OnCustomerPlayed;
                _cardNegotiationService.OnPlayerPlayed -= OnPlayerPlayed;
            }

            // Unsubscribe from all negotiation rounds
            UnsubscribeFromRounds();
        }

        private void OnCustomerChanged(Customer customer)
        {
            // Unsubscribe from existing negotiation rounds
            UnsubscribeFromRounds();

            // Clear existing cards
            ClearExistingCards();

            // Initialize negotiation rounds (based on service initialization)
            InitializeNegotiationRounds();

            // Update initial price display
            UpdateInitialPriceDisplay(customer);

            // Update item info display
            UpdateItemInfoDisplay(customer);

            Debug.Log($"[CardNegotiationController] Player tags: {_cardNegotiationService.PlayerTags?.Count ?? 0}");
            Debug.Log($"[CardNegotiationController] Customer tags: {_cardNegotiationService.CustomerTags?.Count ?? 0}");


            // Create cards for player tags (interactive slots)
            if (_cardNegotiationService.PlayerTags != null)
            {
                foreach (var tag in _cardNegotiationService.PlayerTags)
                {
                    CreateCardForTag(tag, _playerCardContainer, true, true);
                }
            }

            // Create cards for customer tags (non-interactive slots)
            if (_cardNegotiationService.CustomerTags != null)
            {
                foreach (var tag in _cardNegotiationService.CustomerTags)
                {
                    CreateCardForTag(tag, _customerCardContainer, false, false);
                }
            }
        }

        private void ClearExistingCards()
        {
            // Clear existing card slots (which contain cards) from player card container
            if (_playerCardContainer != null)
            {
                for (int i = _playerCardContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _playerCardContainer.GetChild(i);
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            // Clear existing card slots (which contain cards) from customer card container
            if (_customerCardContainer != null)
            {
                for (int i = _customerCardContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _customerCardContainer.GetChild(i);
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void UnsubscribeFromRounds()
        {
            // Unsubscribe from all negotiation rounds using dictionary
            foreach (var roundController in _roundControllers.Values)
            {
                if (roundController != null)
                {
                    roundController.OnMultiplierChanged -= OnRoundMultiplierChanged;
                }
            }
            _roundControllers.Clear();
        }

        private void ClearExistingRounds()
        {
            // Clear existing negotiation rounds from negotiation container
            if (_negotiationContainer != null)
            {
                for (int i = _negotiationContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _negotiationContainer.GetChild(i);
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void CreateCardForTag(BaseTagModel tagModel, Transform container, bool isInteractive, bool destroyOnDragOut = false)
        {
            if (_cardPrefab == null || _cardSlotPrefab == null || container == null) return;

            // First instantiate card slot under specified container
            var cardSlotInstance = _container.InstantiatePrefab(_cardSlotPrefab, container);

            // Set slot interactivity
            var cardSlotController = cardSlotInstance.GetComponent<CardSlotController>();
            if (cardSlotController != null)
            {
                cardSlotController.canReceiveDragged = isInteractive;
            }

            // Then instantiate card as child of the card slot
            var cardInstance = _container.InstantiatePrefab(_cardPrefab, cardSlotInstance.transform);

            // Get CardController and initialize with tag
            var cardController = cardInstance.GetComponent<CardController>();
            if (cardController != null)
            {
                cardController.Init(tagModel);

                // Set card interactivity
                var draggableComponent = cardController.GetComponent<DraggableItemController<BaseTagModel>>();
                if (draggableComponent != null)
                {
                    draggableComponent.canDrag = isInteractive;
                }
            }
        }

        private void UpdateInitialPriceDisplay(Customer customer)
        {
            if (_initialPrice != null && customer?.OwnedItem != null)
            {
                var basePrice = customer.OwnedItem.BasePrice;
                _initialPrice.text = basePrice.ToString();

                // Update negotiated price to initial price
                if (_negotiatedPrice != null)
                {
                    _negotiatedPrice.text = basePrice.ToString("F2");
                }

                // Update paid price from item's purchase price (only if > 0)
                if (_paidPrice != null)
                {
                    bool shouldShow = customer.OwnedItem.PurchasePrice > 0;
                    _paidPrice.gameObject.SetActive(shouldShow);

                    if (shouldShow)
                    {
                        _paidPrice.text = customer.OwnedItem.PurchasePrice.ToString("F2");
                    }
                }
            }
        }

        private void UpdateItemInfoDisplay(Customer customer)
        {
            if (_itemInfoController != null && customer?.OwnedItem != null)
            {
                _itemInfoController.SetItem(customer.OwnedItem);
            }
        }

        private void OnRoundMultiplierChanged(float newMultiplier)
        {
            // Update current price based on multiplier
            UpdateCurrentPrice(newMultiplier);
        }

        private void OnPriceChanged(float newPrice)
        {
            // Update negotiated price display
            if (_negotiatedPrice != null)
            {
                var priceText = newPrice.ToString("F2");
                if (_cardNegotiationService.IsAtMinimumPrice())
                {
                    priceText += " [min]";
                }
                _negotiatedPrice.text = priceText;
            }
        }

        private void OnRoundChanged(int roundNumber)
        {
            Debug.Log($"[CardNegotiationController] Round changed to: {roundNumber}");
        }

        private void OnCustomerPlayed(BaseTagModel customerTag, int roundNumber)
        {
            Debug.Log($"[CardNegotiationController] OnCustomerPlayed: {customerTag.DisplayName} - {roundNumber}");

            // Find the card controller that matches the tag
            var cardControllers = _customerCardContainer.GetComponentsInChildren<CardController>();
            foreach (var cardController in cardControllers)
            {
                if (cardController.Payload == customerTag)
                {
                    // Set the card in the appropriate round
                    if (_roundControllers.ContainsKey(roundNumber))
                    {
                        _roundControllers[roundNumber].SetCustomerCard(cardController);
                    }
                    break;
                }
            }

            // Remove empty customer slots after customer played
            RemoveEmptyCustomerSlots();

            // Start delay and call NextRound
            StartCoroutine(HandleWithDelay(() =>
            {
                Debug.Log($"[CardNegotiationController] Customer played: {customerTag.DisplayName}");
                _cardNegotiationService.NextRound();
            }));
        }

        private void OnPlayerPlayed(BaseTagModel playerTag, int roundNumber)
        {
            Debug.Log($"[CardNegotiationController] OnPlayerPlayed: {playerTag.DisplayName} - {roundNumber}");

            // Remove empty player slots after player played
            RemoveEmptyPlayerSlots();

            // Start delay and call CustomerPlay
            StartCoroutine(HandleWithDelay(() =>
            {
                Debug.Log($"[CardNegotiationController] Player played: {playerTag.DisplayName}");
                _cardNegotiationService.CustomerPlay(playerTag);
            }));
        }

        private System.Collections.IEnumerator HandleWithDelay(System.Action action)
        {
            yield return new WaitForSeconds(1f);
            action?.Invoke();
        }

        private void UpdateCurrentPrice(float multiplier)
        {
            if (_negotiationContainer != null)
            {
                // Get all round controllers and collect multipliers
                var roundControllers = _negotiationContainer.GetComponentsInChildren<NegotiationRoundController>();
                var multipliers = new System.Collections.Generic.List<float>();

                foreach (var roundController in roundControllers)
                {
                    if (roundController != null)
                    {
                        multipliers.Add(roundController.EffectMultiplier);
                    }
                }

                // Use service to update negotiated price
                _cardNegotiationService.UpdateNegotiatedPrice(multipliers);
            }
        }

        /// <summary>
        /// Remove empty slots from player card container
        /// </summary>
        private void RemoveEmptyPlayerSlots()
        {
            if (_playerCardContainer == null) return;

            // Get all card slot controllers in player container
            var slotControllers = _playerCardContainer.GetComponentsInChildren<CardSlotController>();
            
            foreach (var slotController in slotControllers)
            {
                if (slotController != null && slotController.transform.childCount == 0)
                {
                    // Slot is empty, destroy it
                    Destroy(slotController.gameObject);
                }
            }
        }

        /// <summary>
        /// Remove empty slots from customer card container
        /// </summary>
        private void RemoveEmptyCustomerSlots()
        {
            if (_customerCardContainer == null) return;

            // Get all card slot controllers in customer container
            var slotControllers = _customerCardContainer.GetComponentsInChildren<CardSlotController>();
            
            foreach (var slotController in slotControllers)
            {
                if (slotController != null && slotController.transform.childCount == 0)
                {
                    // Slot is empty, destroy it
                    Destroy(slotController.gameObject);
                }
            }
        }

    }
}
