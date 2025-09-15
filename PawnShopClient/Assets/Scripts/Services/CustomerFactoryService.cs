using System;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Repositories;
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
            Debug.Log("[CustomerFactory] GenerateRandomCustomer called");

            var customer = new Customer();

            // Determine customer type based on inventory
            DetermineCustomerType(customer);
            Debug.Log($"[CustomerFactory] Customer type determined: {customer.CustomerType}");

            // Generate random skill levels for all skills
            GenerateRandomSkills(customer);

            // Generate random patience level (50-100)
            GenerateRandomPatience(customer);

            // Generate item based on customer type
            if (customer.CustomerType == CustomerType.Buyer)
            {
                // Buyer wants to buy an item from player's inventory
                customer.OwnedItem = GetRandomItemFromSell();
                Debug.Log($"[CustomerFactory] Buyer customer - selected item from inventory: {customer.OwnedItem?.Name ?? "NULL"}");
            }
            else
            {
                // Seller brings their own item to sell to player
                customer.OwnedItem = _itemRepository.GetRandomItem();
                Debug.Log($"[CustomerFactory] Seller customer - generated random item: {customer.OwnedItem?.Name ?? "NULL"}");
            }

            if (customer.OwnedItem == null)
            {
                Debug.LogError("[CustomerFactory] Failed to get item for customer!");
            }

            return customer;
        }

        private void DetermineCustomerType(Customer customer)
        {
            try
            {
                var sellStorage = _storageLocator.Get(StorageType.SellStorage);
                var sellItemCount = sellStorage.GetOccupiedSlotsCount();

                if (sellItemCount > 0)
                {
                    // 50% chance to be buyer if inventory has items
                    customer.CustomerType = _random.NextDouble() < BUYER_CHANCE ? CustomerType.Buyer : CustomerType.Seller;
                    Debug.Log($"[CustomerFactory] Generated {customer.CustomerType} customer (inventory items: {sellItemCount})");
                }
                else
                {
                    // Always seller if inventory is empty (no items to buy)
                    customer.CustomerType = CustomerType.Seller;
                    Debug.Log($"[CustomerFactory] Generated {customer.CustomerType} customer - inventory is empty, no items to buy");
                }
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

                // Generate random level with minimum of 1 (customers are more experienced)
                // Range: 1 to maxLevel (instead of 0 to maxLevel)
                var randomLevel = _random.Next(1, skillPrototype.maxLevel + 1);

                // Create skill instance with random level
                var skill = new Skill(skillPrototype)
                {
                    Level = randomLevel
                };

                // Add skill to customer
                customer.Skills[skillPrototype.skillType] = skill;

                Debug.Log($"[CustomerFactory] Generated skill {skillPrototype.displayName} (Type: {skillPrototype.skillType}) with level {randomLevel}/{skillPrototype.maxLevel}");
            }

            Debug.Log($"[CustomerFactory] Generated {customer.Skills.Count} random skills for customer (minimum level 1)");
        }

        private void GenerateRandomPatience(Customer customer)
        {
            // Generate random patience level between 50 and 100
            // Lower patience means customer will leave faster
            customer.Patience = (float)(_random.NextDouble() * 50 + 50);
            Debug.Log($"[CustomerFactory] Generated patience level: {customer.Patience:F1}/100");
        }

        /// <summary>
        /// Get a random item from player's inventory for buyer customers
        /// </summary>
        /// <returns>Random item from inventory or null if inventory is empty</returns>
        private ItemModel GetRandomItemFromSell()
        {
            try
            {
                var sellStorage = _storageLocator.Get(StorageType.SellStorage);
                var itemsForSell = sellStorage.All.Where(item => item.Value != null).ToList();

                if (!itemsForSell.Any())
                {
                    Debug.LogWarning("[CustomerFactory] Cannot create buyer customer - inventory is empty");
                    return null;
                }

                // Select random item from occupied slots
                var randomIndex = _random.Next(itemsForSell.Count);
                var selectedItem = itemsForSell[randomIndex].Value;

                Debug.Log($"[CustomerFactory] Selected item from inventory: {selectedItem.Name} (ID: {selectedItem.Id})");
                return selectedItem;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomerFactory] Failed to get item from inventory: {ex.Message}");
                return null;
            }
        }
    }
}