using System;
using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface ICustomerService
    {
        Customer CurrentCustomer { get; }
        void NextCustomer();
        long EvaluateCurrentItem();
        event Action<Customer> OnCustomerChanged;
    }
}