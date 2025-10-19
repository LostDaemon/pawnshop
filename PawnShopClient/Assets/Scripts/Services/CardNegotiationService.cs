using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;
using PawnShop.Models.Characters;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Models.Npc;

namespace PawnShop.Services
{
    public class CardNegotiationService : ICardNegotiationService
    {
        private const float MIN_PRICE_PERCENTAGE = 0.1f;
        private const int NEGOTIATION_ROUNDS_COUNT = 5;

        private readonly ICustomerService _customerService;
        private Dictionary<int, NegotiationRound> _negotiationRounds;

        public Customer CurrentCustomer { get; private set; }
        public float BasePrice { get; private set; }
        public float CurrentNegotiatedPrice { get; private set; }
        public Dictionary<int, NegotiationRound> NegotiationRounds => _negotiationRounds;

        // Item tags for player and customer
        public List<BaseTagModel> PlayerTags { get; private set; } = new List<BaseTagModel>();
        public List<BaseTagModel> CustomerTags { get; private set; } = new List<BaseTagModel>();

        // Round counter
        public int CurrentRound { get; private set; } = 0;

        public event Action<Customer> OnCustomerChanged;
        public event Action<float> OnPriceChanged;
        public event Action<int> OnRoundChanged;
        public event Action<BaseTagModel, int> OnCustomerPlayed;
        public event Action<BaseTagModel, int> OnPlayerPlayed;

        [Inject]
        public CardNegotiationService(ICustomerService customerService)
        {
            _customerService = customerService;
            _negotiationRounds = new Dictionary<int, NegotiationRound>();
            _customerService.OnCustomerChanged += OnCustomerServiceChanged;

            // Initialize with current customer if one already exists
            if (_customerService.CurrentCustomer != null)
            {
                SetCustomer(_customerService.CurrentCustomer);
            }
        }

        // Public interface methods
        public float CalculateNegotiatedPrice(List<float> multipliers)
        {
            float totalEffect = GetTotalEffect(multipliers);
            float newPrice = BasePrice * (1f + totalEffect);
            return ApplyPriceConstraints(newPrice);
        }

        public float ApplyPriceConstraints(float price)
        {
            float minPrice = BasePrice * MIN_PRICE_PERCENTAGE;
            return Mathf.Max(price, minPrice);
        }

        public float GetTotalEffect(List<float> multipliers)
        {
            float totalEffect = 0f;
            if (multipliers != null)
            {
                foreach (var multiplier in multipliers)
                {
                    totalEffect += multiplier;
                }
            }
            return totalEffect;
        }

        public void UpdateNegotiatedPrice(List<float> multipliers)
        {
            CurrentNegotiatedPrice = CalculateNegotiatedPrice(multipliers);
            OnPriceChanged?.Invoke(CurrentNegotiatedPrice);
        }

        public bool IsAtMinimumPrice()
        {
            float minPrice = BasePrice * MIN_PRICE_PERCENTAGE;
            return CurrentNegotiatedPrice <= minPrice;
        }

        public void PlayerPlay(BaseTagModel tag)
        {
            if (tag == null) return;
            // Remove the played tag from player tags
            PlayerTags.Remove(tag);
            // Invoke player played event
            OnPlayerPlayed?.Invoke(tag, CurrentRound);
        }

        public void NextRound()
        {
            // Increment round counter and invoke event
            CurrentRound++;
            OnRoundChanged?.Invoke(CurrentRound);
        }

        public void ClearCustomer()
        {
            CurrentCustomer = null;
            BasePrice = 0f;
            CurrentNegotiatedPrice = 0f;
            OnCustomerChanged?.Invoke(null);
        }

        public void CustomerPlay(BaseTagModel playerTag)
        {
            if (CurrentCustomer == null || CustomerTags == null || CustomerTags.Count == 0) return;

            // Find the card with closest absolute value to player's card
            BaseTagModel closestCard = null;
            float playerMultiplier = playerTag.PriceMultiplier;
            float minDifference = float.MaxValue;

            foreach (var customerTag in CustomerTags)
            {
                float difference = Mathf.Abs(customerTag.PriceMultiplier - playerMultiplier);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestCard = customerTag;
                }
            }

            if (closestCard != null)
            {
                // Remove the played card from customer tags
                CustomerTags.Remove(closestCard);

                Debug.Log($"[CardNegotiationService] Customer played closest card: {closestCard.DisplayName} (difference: {minDifference:F2}). Remaining customer tags: {CustomerTags.Count}");

                // Invoke customer played event
                OnCustomerPlayed?.Invoke(closestCard, CurrentRound);
            }
            else
            {
                Debug.Log("[CardNegotiationService] Customer has no cards to play");
            }
        }

        // Private methods
        private void SetCustomer(Customer customer)
        {
            CurrentCustomer = customer;

            // Clear previous item tags
            PlayerTags.Clear();
            CustomerTags.Clear();

            // Reset round counter
            CurrentRound = 0;

            if (customer?.OwnedItem != null)
            {
                BasePrice = customer.OwnedItem.BasePrice;
                CurrentNegotiatedPrice = customer.OwnedItem.BasePrice;

                // Get item tags known to player and customer
                UpdateItemTags(customer.OwnedItem);
            }

            // Initialize negotiation rounds before invoking event
            InitializeRounds();

            OnCustomerChanged?.Invoke(customer);
        }

        private void InitializeRounds()
        {
            _negotiationRounds.Clear();

            for (int i = 1; i <= NEGOTIATION_ROUNDS_COUNT; i++)
            {
                _negotiationRounds[i] = new NegotiationRound
                {
                    RoundId = i,
                    PlayerTag = null,
                    CustomerTag = null,
                    Effect = 0f
                };
            }
        }

        private void UpdateItemTags(ItemModel item)
        {
            if (item?.Tags == null || CurrentCustomer == null) return;

            foreach (var tag in item.Tags)
            {
                // Ignore tags with zero multiplier (they will be removed from the game)
                if (tag.PriceMultiplier == 1f) continue;

                // Determine which tags go to player and customer based on customer type and tag effect
                if (CurrentCustomer.CustomerType == NpcType.Seller)
                {
                    // If customer is seller:
                    // - Customer gets tags with positive effect (beneficial for them)
                    // - Player gets tags with negative effect (beneficial for player)
                    //if (tag.PriceMultiplier > 0f && tag.IsRevealedToCustomer)
                    if (tag.PriceMultiplier > 1f)
                    {
                        CustomerTags.Add(tag);
                        Debug.Log($"[CardNegotiationService] Added tag to customer tags: {tag.DisplayName}");
                    }
                    //if (tag.PriceMultiplier < 0f && tag.IsRevealedToPlayer)
                    if (tag.PriceMultiplier < 1f)
                    {
                        PlayerTags.Add(tag);
                        Debug.Log($"[CardNegotiationService] Added tag to player tags: {tag.DisplayName}");
                    }
                }
                else if (CurrentCustomer.CustomerType == NpcType.Buyer)
                {
                    // If customer is buyer:
                    // - Customer gets tags with negative effect (beneficial for them)
                    // - Player gets tags with positive effect (beneficial for player)
                    //if (tag.PriceMultiplier < 0f && tag.IsRevealedToCustomer)
                    if (tag.PriceMultiplier < 1f)
                    {
                        CustomerTags.Add(tag);
                    }
                    // if (tag.PriceMultiplier > 0f && tag.IsRevealedToPlayer)
                    if (tag.PriceMultiplier > 1f)
                    {
                        PlayerTags.Add(tag);
                    }
                }
            }
        }

        private void OnCustomerServiceChanged(Customer customer)
        {
            SetCustomer(customer);
        }

        public void Dispose()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerServiceChanged;
            }
        }
    }
}
