using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerFactoryService : ICustomerFactoryService
{
    private readonly System.Random _random = new();
    private readonly IItemRepositoryService _itemRepository;
    private readonly ISkillRepositoryService _skillRepository;

    public CustomerFactoryService(
        IItemRepositoryService itemRepository,
        ISkillRepositoryService skillRepository)
    {
        _itemRepository = itemRepository;
        _skillRepository = skillRepository;
    }

    public Customer GenerateRandomCustomer()
    {
        var customer = new Customer(null, _random.Next(0, 101) / 100f)
        {
            UncertaintyLevel = _random.Next(0, 101) / 100f
        };

        // Generate random skill levels for all skills
        GenerateRandomSkills(customer);

        customer.OwnedItem = _itemRepository.GetRandomItem();
        Debug.Log($"[CustomerFactory] Generated customer with item: {customer.OwnedItem.Name}");
        return customer;
    }

    private void GenerateRandomSkills(Customer customer)
    {
        // Get all available skills from repository
        var allSkills = _skillRepository.GetAllSkills();
        
        Debug.Log($"[CustomerFactory] Found {allSkills.Count} skill prototypes in repository");
        
        foreach (var skillPrototype in allSkills)
        {
            if (skillPrototype.skillType == SkillType.Undefined)
                continue;

            // Generate random level from 0 to maxLevel
            var randomLevel = _random.Next(0, skillPrototype.maxLevel + 1);
            
            // Create skill instance with random level
            var skill = new Skill(skillPrototype)
            {
                Level = randomLevel
            };
            
            // Add skill to customer
            customer.Skills[skillPrototype.skillType] = skill;
            
            Debug.Log($"[CustomerFactory] Generated skill {skillPrototype.displayName} (Type: {skillPrototype.skillType}) with level {randomLevel}/{skillPrototype.maxLevel}");
        }
        
        Debug.Log($"[CustomerFactory] Generated {customer.Skills.Count} random skills for customer");
    }
}