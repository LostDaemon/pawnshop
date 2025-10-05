using UnityEngine;
using UnityEngine.UI;
using PawnShop.Services;
using Zenject;
using PawnShop.Models.Tags;
using PawnShop.Models.Characters;
using PawnShop.Models;
using System.Collections.Generic;
using PawnShop.Controllers.DragNDrop;

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

        [Inject]
        public void Construct(ICardNegotiationService cardNegotiationService, DiContainer container)
        {
            _cardNegotiationService = cardNegotiationService;
            _container = container;

            // Subscribe to service events
            _cardNegotiationService.OnCustomerChanged += OnCustomerChanged;
            _cardNegotiationService.OnPriceChanged += OnPriceChanged;
        }

        private void InitializeNegotiationRounds()
        {
            if (_negotiationRoundPrefab == null || _negotiationContainer == null) return;

            // Clear existing rounds first
            ClearExistingRounds();

            // Get rounds from service
            var rounds = _cardNegotiationService.NegotiationRounds;
            if (rounds == null) return;

            foreach (var round in rounds.Values)
            {
                var roundInstance = _container.InstantiatePrefab(_negotiationRoundPrefab, _negotiationContainer);
                var roundController = roundInstance.GetComponent<NegotiationRoundController>();
                if (roundController != null)
                {
                    // Subscribe to multiplier changes
                    roundController.OnMultiplierChanged += OnRoundMultiplierChanged;
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

            Debug.Log($"[CardNegotiationController] Player tags: {_cardNegotiationService.PlayerTags?.Count ?? 0}");
            Debug.Log($"[CardNegotiationController] Customer tags: {_cardNegotiationService.CustomerTags?.Count ?? 0}");


            // Create cards for player tags (interactive slots)
            if (_cardNegotiationService.PlayerTags != null)
            {
                foreach (var tag in _cardNegotiationService.PlayerTags)
                {
                    CreateCardForTag(tag, _playerCardContainer, true);
                }
            }

            // Create cards for customer tags (non-interactive slots)
            if (_cardNegotiationService.CustomerTags != null)
            {
                foreach (var tag in _cardNegotiationService.CustomerTags)
                {
                    CreateCardForTag(tag, _customerCardContainer, false);
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
            // Unsubscribe from all negotiation rounds
            if (_negotiationContainer != null)
            {
                var roundControllers = _negotiationContainer.GetComponentsInChildren<NegotiationRoundController>();
                foreach (var roundController in roundControllers)
                {
                    if (roundController != null)
                    {
                        roundController.OnMultiplierChanged -= OnRoundMultiplierChanged;
                    }
                }
            }
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

        private void CreateCardForTag(BaseTagModel tagModel, Transform container, bool isInteractive)
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
    }
}
