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
        var customer = new Customer();

        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            customer.Skills.Set(skill, _random.Next(0, 6));
        }

        customer.OwnedItem = _itemRepository.GetRandomItem();
        Debug.Log($"[CustomerFactory] Generated customer with item: {customer.OwnedItem.Name}");
        return customer;
    }
}