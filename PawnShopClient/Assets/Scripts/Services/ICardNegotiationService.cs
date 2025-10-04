using System;
using System.Collections.Generic;
using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface ICardNegotiationService : IDisposable
    {
        // Customer management
        Customer CurrentCustomer { get; }
        
        // Price state management
        float BasePrice { get; }
        float CurrentNegotiatedPrice { get; }
        
        // Price calculation methods
        float CalculateNegotiatedPrice(List<float> multipliers);
        float ApplyPriceConstraints(float price);
        float GetTotalEffect(List<float> multipliers);
        
        // Price state methods
        void UpdateNegotiatedPrice(List<float> multipliers);
        bool IsAtMinimumPrice();
        
        // Customer management methods
        void SetCustomer(Customer customer);
        void ClearCustomer();
        
        // Events
        event Action<Customer> OnCustomerChanged;
        event Action<float> OnPriceChanged;
    }
}
