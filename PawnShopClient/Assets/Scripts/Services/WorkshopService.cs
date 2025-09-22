using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Repositories;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class WorkshopService : BaseProcessingService, IWorkshopService
    {
        private readonly IWalletService _walletService;

        private const int RECYCLE_DURATION_TICKS = 1; // Recycle takes 1 tick (1 game minute)
        private const float REPAIR_MATERIAL_RATIO = 0.2f; // Repair requires 1/5 of materials

        [Inject]
        public WorkshopService(IStorageLocatorService storageLocatorService, ITimeService timeService, ITagService tagService, IWalletService walletService)
            : base(storageLocatorService, timeService, tagService, StorageType.WorkshopStorage)
        {
            _walletService = walletService;
        }

        public bool IsDestroyed(ItemModel item)
        {
            if (item == null) return false;

            // Get destroyed tag prototype
            var destroyedTagPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.ConditionDestroyed);
            if (destroyedTagPrototype == null) return false;

            // Check if item has "Destroyed" tag by ClassId
            return item.Tags.Any(tag => tag.ClassId == destroyedTagPrototype.ClassId);
        }

        public override bool CanPerformTask(ItemModel item, ProcessingType taskType)
        {
            if (item == null) return false;

            // Recycle is always available
            if (taskType == ProcessingType.Recycle)
                return true;

            // Cannot perform task on destroyed items
            if (IsDestroyed(item)) return false;

            // For repair, check if we have enough materials (1/5 of each material in the item)
            if (taskType == ProcessingType.Repair)
            {
                if (!HasEnoughMaterialsForRepair(item))
                    return false;
            }

            // Check if item has tags with the specified ProcessingType that are revealed to player
            return HasKnownTagsByProcessType(item, taskType);
        }

        private void WithdrawRepairMaterials(ItemModel item)
        {
            if (item?.Materials == null || item.Materials.Count == 0)
                return; // No materials to withdraw

            foreach (var material in item.Materials)
            {
                if (material.MaterialType == CurrencyType.Undefined || material.Quantity <= 0)
                    continue;

                // Calculate amount to withdraw (1/5 of the material)
                int withdrawAmount = Mathf.CeilToInt(material.Quantity * REPAIR_MATERIAL_RATIO);

                // Withdraw materials from wallet
                bool success = _walletService.TransactionAttempt(material.MaterialType, -withdrawAmount);
                if (success)
                {
                    Debug.Log($"[WorkshopService] Withdrew {withdrawAmount} {material.MaterialType} for repair");
                }
                else
                {
                    Debug.LogWarning($"[WorkshopService] Failed to withdraw {withdrawAmount} {material.MaterialType} for repair");
                }
            }
        }

        private bool HasEnoughMaterialsForRepair(ItemModel item)
        {
            if (item?.Materials == null || item.Materials.Count == 0)
                return true; // No materials required

            foreach (var material in item.Materials)
            {
                if (material.MaterialType == CurrencyType.Undefined || material.Quantity <= 0)
                    continue;

                // Check if we have at least 1/5 of the required material
                int requiredAmount = Mathf.CeilToInt(material.Quantity * REPAIR_MATERIAL_RATIO);
                long availableAmount = _walletService.GetBalance(material.MaterialType);

                if (availableAmount < requiredAmount)
                {
                    Debug.Log($"[WorkshopService] Not enough {material.MaterialType} for repair. Required: {requiredAmount}, Available: {availableAmount}");
                    return false;
                }
            }

            return true;
        }

        public new void ScheduleTask(ProcessingType taskType)
        {
            var item = _storage.Get(0); // Get item from slot 0

            if (item == null)
            {
                Debug.LogWarning($"[WorkshopService] Cannot schedule task - no item in workshop slot 0");
                return;
            }

            // Special handling for Recycle - execute immediately
            if (taskType == ProcessingType.Recycle)
            {
                ProcessItemByType(item, ProcessingType.Recycle);
                Debug.Log($"[WorkshopService] Recycle completed immediately for {item.Name}");
                return;
            }

            // Use base implementation for other tasks
            base.ScheduleTask(taskType);
        }

        protected override GameTime CalculateReadyTime(ProcessingType taskType)
        {
            // Recycle should take 1 minute
            if (taskType == ProcessingType.Recycle)
            {
                var currentTime = _timeService.CurrentTime;
                return new GameTime(currentTime.Day, currentTime.Hour, currentTime.Minute + RECYCLE_DURATION_TICKS);
            }

            // Use base implementation for other tasks
            return base.CalculateReadyTime(taskType);
        }

        protected override void ProcessItemByType(ItemModel item, ProcessingType processingType)
        {
            switch (processingType)
            {
                case ProcessingType.Repair:
                    ProcessRepair(item);
                    break;
                case ProcessingType.Recycle:
                    ProcessRecycle(item);
                    break;
                default:
                    ProcessDefaultTask(item, processingType);
                    break;
            }
        }

        private void ProcessRepair(ItemModel item)
        {
            if (item == null) return;

            // Always withdraw materials for repair attempt
            WithdrawRepairMaterials(item);

            // Find all condition tags that can be upgraded
            var conditionTags = item.Tags.Where(tag => tag.TagType == TagType.Condition).ToList();

            foreach (var tag in conditionTags)
            {
                // Try to upgrade first
                if (TryUpgradeTag(item, tag.TagType))
                {
                    Debug.Log($"[WorkshopService] Repair successful - upgraded {tag.TagType}");
                    return; // Success, stop processing
                }

                // If upgrade failed, try to degrade
                if (TryDegradeTag(item, tag.TagType))
                {
                    Debug.Log($"[WorkshopService] Repair CRITICALLY FAILED - Additional damage made {tag.TagType}");
                    return; // Critically Failed, stop processing
                }
            }

            Debug.Log($"[WorkshopService] Repair failed - no changes made");
        }

        private void ProcessRecycle(ItemModel item)
        {
            if (item == null) return;

            Debug.Log($"[WorkshopService] Starting recycle process for {item.Name}");

            // Add materials from item to player's wallet
            foreach (var material in item.Materials)
            {
                if (material.MaterialType != CurrencyType.Undefined && material.Quantity > 0)
                {
                    bool success = _walletService.TransactionAttempt(material.MaterialType, material.Quantity);
                    if (success)
                    {
                        Debug.Log($"[WorkshopService] Added {material.Quantity} {material.MaterialType} to wallet");
                    }
                    else
                    {
                        Debug.LogWarning($"[WorkshopService] Failed to add {material.Quantity} {material.MaterialType} to wallet");
                    }
                }
            }

            // Remove item from storage after successful recycle
            _storage.Withdraw(0);
            Debug.Log($"[WorkshopService] Item {item.Name} successfully recycled and removed from storage");
        }

        protected override string GetServiceName()
        {
            return "WorkshopService";
        }

        protected override string GetStorageName()
        {
            return "workshop";
        }
    }
}
