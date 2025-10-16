using System;
using PawnShop.Models.Characters;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface ICustomerService
    {
        Customer CurrentCustomer { get; }
        void NextCustomer();
        void RequestSkip();
        void ClearCustomer();
        void ChangeCustomerPatience(float changeAmount);
        void SetCustomerAction(NpcAction action);
        event Action<Customer> OnCustomerChanged;
        event Action<float> OnPatienceChanged;
        event Action<NpcAction> OnCustomerActionChanged;
    }
}