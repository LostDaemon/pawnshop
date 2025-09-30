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
        private ICustomerService _customerService;
        private DiContainer _container;

        private float _currentPrice;

        [Inject]
        public void Construct(ICardNegotiationService cardNegotiationService, ICustomerService customerService, DiContainer container)
        {
            _cardNegotiationService = cardNegotiationService;
            _customerService = customerService;
            _container = container;

            // Initialize negotiation rounds
            InitializeNegotiationRounds();

            // Subscribe to customer changes
            _customerService.OnCustomerChanged += OnCustomerChanged;
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
            // Unsubscribe from events to prevent memory leaks
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
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

            // Update initial price from customer service
            UpdateInitialPrice(customer);

            // TODO: Implement card negotiation UI logic

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

        private void UpdateInitialPrice(Customer customer)
        {
            if (_initialPrice != null && customer?.OwnedItem != null)
            {
                // Get base price from customer service (without tag multipliers)
                var basePrice = customer.OwnedItem.BasePrice;
                _currentPrice = basePrice;
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

        private void UpdateCurrentPrice(float multiplier)
        {
            if (_negotiationContainer != null)
            {
                // Get all round controllers and calculate total effect
                var roundControllers = _negotiationContainer.GetComponentsInChildren<NegotiationRoundController>();
                float totalEffect = 0f; // Start with 0 for addition
                var effectParts = new System.Collections.Generic.List<string>();

                foreach (var roundController in roundControllers)
                {
                    if (roundController != null)
                    {
                        // Add the effect directly (already represents the effect)
                        float roundEffect = roundController.EffectMultiplier;
                        totalEffect += roundEffect;
                        
                        if (roundEffect != 0f)
                        {
                            effectParts.Add($"{roundEffect:+0.00;-0.00;0.00}");
                        }
                    }
                }

                // Calculate new price: base price * (1 + total effect)
                var newPrice = _currentPrice * (1f + totalEffect);
                
                // Apply minimum price constraint (10% of base price)
                var minPrice = _currentPrice * MIN_PRICE_PERCENTAGE;
                bool isAtMinimum = newPrice < minPrice;
                if (isAtMinimum)
                {
                    newPrice = minPrice;
                }


                // Update negotiated price display
                if (_negotiatedPrice != null)
                {
                    var priceText = newPrice.ToString("F2");
                    if (isAtMinimum)
                    {
                        priceText += " [min]";
                    }
                    _negotiatedPrice.text = priceText;
                }
            }
        }
    }
}
