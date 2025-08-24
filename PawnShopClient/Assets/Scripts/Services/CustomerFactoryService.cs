using System;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Repositories;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Services
{
    public class CustomerFactoryService : ICustomerFactoryService
    {
        private const float BUYER_CHANCE = 0.5f;
        
        private readonly System.Random _random = new();
        private readonly IItemRepository _itemRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IStorageLocatorService _storageLocator;

        public CustomerFactoryService(
            IItemRepository itemRepository,
            ISkillRepository skillRepository,
            IStorageLocatorService storageLocator)
        {
            _itemRepository = itemRepository;
            _skillRepository = skillRepository;
            _storageLocator = storageLocator;
        }

        public Customer GenerateRandomCustomer()
        {
            var customer = new Customer()
            {
                UncertaintyLevel = _random.Next(0, 101) / 100f
            };

            // Determine customer type based on inventory
            DetermineCustomerType(customer);

            // Generate random skill levels for all skills
            GenerateRandomSkills(customer);

            customer.OwnedItem = _itemRepository.GetRandomItem();
            Debug.Log($"[CustomerFactory] Generated customer with item: {customer.OwnedItem.Name}");
            return customer;
        }

        private void DetermineCustomerType(Customer customer)
        {
            try
            {
                var inventoryStorage = _storageLocator.Get(StorageType.InventoryStorage);
                var inventoryItemCount = inventoryStorage.All.Count;

                if (inventoryItemCount > 0)
                {
                    // 50% chance to be buyer if inventory has items
                    customer.CustomerType = _random.NextDouble() < BUYER_CHANCE ? CustomerType.Buyer : CustomerType.Seller;
                }
                else
                {
                    // Always seller if inventory is empty
                    customer.CustomerType = CustomerType.Seller;
                }

                Debug.Log($"[CustomerFactory] Generated {customer.CustomerType} customer (inventory items: {inventoryItemCount})");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CustomerFactory] Failed to determine customer type: {ex.Message}. Defaulting to Seller.");
                customer.CustomerType = CustomerType.Seller;
            }
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
}