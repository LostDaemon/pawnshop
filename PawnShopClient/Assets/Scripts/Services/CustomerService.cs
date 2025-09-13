using System;
using PawnShop.Models.Characters;
using UnityEngine;

namespace PawnShop.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerFactoryService _customerFactory;
        private readonly System.Random _random = new();
        public Customer CurrentCustomer { get; private set; }
        public event Action<Customer> OnCustomerChanged;

        public CustomerService(ICustomerFactoryService customerFactory)
        {
            _customerFactory = customerFactory;
        }

        public void ShowNextCustomer()
        {
            Debug.Log("[CustomerService] ShowNextCustomer called");
            var customer = _customerFactory.GenerateRandomCustomer();
            Debug.Log($"[CustomerService] Generated customer: Type={customer?.CustomerType}, Item={customer?.OwnedItem?.Name}");
            CurrentCustomer = customer;
            OnCustomerChanged?.Invoke(CurrentCustomer);
        }

        public long EvaluateCurrentItem()
        {
            if (CurrentCustomer == null || CurrentCustomer.OwnedItem == null)
                return 0;

            return (long)(CurrentCustomer.OwnedItem.BasePrice * _random.Next(60, 86) / 100f);
        }
    }
}