using System;
using UnityEngine;


public class CustomerFactoryService : ICustomerFactoryService
{
    private readonly System.Random _random = new();
    private readonly IItemRepositoryService _itemRepository;

    public CustomerFactoryService(IItemRepositoryService itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public Customer GenerateRandomCustomer()
    {
        var customer = new Customer(null, _random.Next(0, 101) / 100f)
        {
            UncertaintyLevel = _random.Next(0, 101) / 100f
        };

        customer.OwnedItem = _itemRepository.GetRandomItem();
        Debug.Log($"[CustomerFactory] Generated customer with item: {customer.OwnedItem.Name}");
        return customer;
    }
}