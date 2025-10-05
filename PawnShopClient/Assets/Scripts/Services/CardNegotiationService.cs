using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;
using PawnShop.Models.Characters;
using PawnShop.Models;
using PawnShop.Models.Tags;

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

        public event Action<Customer> OnCustomerChanged;
        public event Action<float> OnPriceChanged;

        [Inject]
        public CardNegotiationService(ICustomerService customerService)
        {
            _customerService = customerService;
            _negotiationRounds = new Dictionary<int, NegotiationRound>();
            _customerService.OnCustomerChanged += OnCustomerServiceChanged;
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

       

        public void ClearCustomer()
        {
            CurrentCustomer = null;
            BasePrice = 0f;
            CurrentNegotiatedPrice = 0f;
            OnCustomerChanged?.Invoke(null);
        }

        public void Dispose()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerServiceChanged;
            }
        }

        // Private methods
        private void SetCustomer(Customer customer)
        {
            CurrentCustomer = customer;
            
            // Clear previous item tags
            PlayerTags.Clear();
            CustomerTags.Clear();
            
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
                if (CurrentCustomer.CustomerType == CustomerType.Seller)
                {
                    // If customer is seller:
                    // - Customer gets tags with positive effect (beneficial for them)
                    // - Player gets tags with negative effect (beneficial for player)
                    //if (tag.PriceMultiplier > 0f && tag.IsRevealedToCustomer)
                    if (tag.PriceMultiplier > 1f )
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
                else if (CurrentCustomer.CustomerType == CustomerType.Buyer)
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
    }
}