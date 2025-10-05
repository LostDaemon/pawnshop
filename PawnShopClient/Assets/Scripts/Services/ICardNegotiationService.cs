using System;
using System.Collections.Generic;
using PawnShop.Models.Characters;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public interface ICardNegotiationService : IDisposable
    {
        // Customer management
        Customer CurrentCustomer { get; }
        
        // Price state management
        float BasePrice { get; }
        float CurrentNegotiatedPrice { get; }
        
        // Negotiation rounds
        Dictionary<int, NegotiationRound> NegotiationRounds { get; }
        
        // Item tags for player and customer
        List<BaseTagModel> PlayerTags { get; }
        List<BaseTagModel> CustomerTags { get; }
        
        // Price calculation methods
        float CalculateNegotiatedPrice(List<float> multipliers);
        float ApplyPriceConstraints(float price);
        float GetTotalEffect(List<float> multipliers);
        
        // Price state methods
        void UpdateNegotiatedPrice(List<float> multipliers);
        bool IsAtMinimumPrice();
        
        // Customer management methods
        void ClearCustomer();
        
        // Events
        event Action<Customer> OnCustomerChanged;
        event Action<float> OnPriceChanged;
    }
}
