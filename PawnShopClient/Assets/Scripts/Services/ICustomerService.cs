using System;
using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface ICustomerService
    {
        Customer CurrentCustomer { get; }
        void NextCustomer();
        void RequestSkip();
        void ClearCustomer();
        void ChangeCustomerPatience(float changeAmount);
        event Action<Customer> OnCustomerChanged;
        event Action<float> OnPatienceChanged;
    }
}