using System;
using PawnShop.Models.Characters;

namespace PawnShop.Services
{
    public interface ICustomerService
    {
        Customer CurrentCustomer { get; }
        void ShowNextCustomer();
        long EvaluateCurrentItem();
        event Action<Customer> OnCustomerChanged;
    }
}