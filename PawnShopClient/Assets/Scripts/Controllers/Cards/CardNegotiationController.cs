using UnityEngine;
using UnityEngine.UI;
using PawnShop.Services;
using Zenject;
using PawnShop.Models.Tags;
using PawnShop.Models.Characters;
using System.Collections.Generic;

namespace PawnShop.Controllers.Cards
{
    public class CardNegotiationController : MonoBehaviour
    {
        private const int NEGOTIATION_ROUNDS_COUNT = 5;
        private const float MIN_PRICE_PERCENTAGE = 0.1f; // 10% of base price

        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _cardSlotPrefab;
        [SerializeField] private GameObject _negotiationRoundPrefab;
        [SerializeField] private Transform _playerCardContainer;
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

            // Initialize negotiation rounds
            InitializeNegotiationRounds();

            // Subscribe to service events
            _cardNegotiationService.OnCustomerChanged += OnCustomerChanged;
            _cardNegotiationService.OnPriceChanged += OnPriceChanged;
        }

        private void InitializeNegotiationRounds()
        {
            if (_negotiationRoundPrefab == null || _negotiationContainer == null) return;

            // Instantiate negotiation rounds
            for (int i = 0; i < NEGOTIATION_ROUNDS_COUNT; i++)
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

        private void OnCustomerChanged(Customer customer)
        {
            // Clear existing cards
            ClearExistingCards();

            // Update initial price display
            UpdateInitialPriceDisplay(customer);

            // Iterate through customer's item tags and create cards
            if (customer?.OwnedItem?.Tags != null)
            {
                foreach (var tag in customer.OwnedItem.Tags)
                {
                    CreateCardForTag(tag);
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
        }

        private void CreateCardForTag(BaseTagModel tagModel)
        {
            if (_cardPrefab == null || _cardSlotPrefab == null || _playerCardContainer == null) return;

            // First instantiate card slot under player card container
            var cardSlotInstance = _container.InstantiatePrefab(_cardSlotPrefab, _playerCardContainer);

            // Then instantiate card as child of the card slot
            var cardInstance = _container.InstantiatePrefab(_cardPrefab, cardSlotInstance.transform);

            // Get CardController and initialize with tag
            var cardController = cardInstance.GetComponent<CardController>();
            if (cardController != null)
            {
                cardController.Init(tagModel);
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
