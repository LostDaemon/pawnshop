using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;
using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public class CardNegotiationService : ICardNegotiationService
    {
        private const float MIN_PRICE_PERCENTAGE = 0.1f;

        private readonly ICustomerService _customerService;

        public Customer CurrentCustomer { get; private set; }
        public float BasePrice { get; private set; }
        public float CurrentNegotiatedPrice { get; private set; }

        public event Action<Customer> OnCustomerChanged;
        public event Action<float> OnPriceChanged;

        [Inject]
        public CardNegotiationService(ICustomerService customerService)
        {
            _customerService = customerService;
            _customerService.OnCustomerChanged += OnCustomerServiceChanged;
        }

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

        public void SetCustomer(Customer customer)
        {
            CurrentCustomer = customer;
            if (customer?.OwnedItem != null)
            {
                BasePrice = customer.OwnedItem.BasePrice;
                CurrentNegotiatedPrice = customer.OwnedItem.BasePrice;
            }
            OnCustomerChanged?.Invoke(customer);
        }

        public void ClearCustomer()
        {
            CurrentCustomer = null;
            BasePrice = 0f;
            CurrentNegotiatedPrice = 0f;
            OnCustomerChanged?.Invoke(null);
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