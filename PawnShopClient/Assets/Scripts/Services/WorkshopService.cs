using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Repositories;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class WorkshopService : BaseProcessingService, IWorkshopService
    {
        [Inject]
        public WorkshopService(IStorageLocatorService storageLocatorService, ITimeService timeService, ITagService tagService)
            : base(storageLocatorService, timeService, tagService, StorageType.WorkshopStorage)
        {
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

            // Cannot perform task on destroyed items
            if (IsDestroyed(item)) return false;

            // Check if item has tags with the specified ProcessingType that are revealed to player
            return HasKnownTagsByProcessType(item, taskType);
        }

        protected override void ProcessItemByType(ItemModel item, ProcessingType processingType)
        {
            switch (processingType)
            {
                case ProcessingType.Repair:
                    ProcessRepair(item);
                    break;
                default:
                    ProcessDefaultTask(item, processingType);
                    break;
            }
        }

        private void ProcessRepair(ItemModel item)
        {
            if (item == null) return;

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
                    Debug.Log($"[WorkshopService] Repair partially successful - degraded {tag.TagType}");
                    return; // Partial success, stop processing
                }
            }

            Debug.Log($"[WorkshopService] Repair failed - no changes made");
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
