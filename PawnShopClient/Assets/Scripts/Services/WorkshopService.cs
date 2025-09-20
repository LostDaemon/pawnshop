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
    public class WorkshopService : IWorkshopService
    {
        private const float DEGRADE_CHANCE = 0.25f; // 25% chance that degrade doesn't happen
        private const float DEFAULT_TASK_SUCCESS_CHANCE = 0.8f; // 80% chance for default tasks to succeed

        private readonly ISlotStorageService<ItemModel> _workshopStorage;
        private readonly IStorageLocatorService _storageLocatorService;
        private readonly ITimeService _timeService;
        private readonly ITagService _tagService;
        private readonly List<ProcessingTask> _scheduledTasks = new List<ProcessingTask>();

        public ItemModel CurrentItem { get; private set; }
        public event System.Action<ItemModel> OnItemAdded;
        public event System.Action<ItemModel> OnItemRemoved;
        public event System.Action<ProcessingTask> OnTaskCompleted;

        [Inject]
        public WorkshopService(IStorageLocatorService storageLocatorService, ITimeService timeService, ITagService tagService)
        {
            _storageLocatorService = storageLocatorService;
            _timeService = timeService;
            _tagService = tagService;
            _workshopStorage = _storageLocatorService.Get(StorageType.WorkshopStorage);

            // Subscribe to storage changes
            _workshopStorage.OnItemChanged += OnWorkshopItemChanged;

            // Subscribe to time changes
            _timeService.OnTimeChanged += OnTimeChanged;
        }

        public void ScheduleTask(ProcessingType taskType)
        {
            var item = _workshopStorage.Get(0); // Get item from slot 0

            if (item == null)
            {
                Debug.LogWarning("[WorkshopService] Cannot schedule task - no item in workshop slot 0");
                return;
            }

            // Special handling for Recycle - always available
            if (taskType == ProcessingType.Recycle)
            {
                ScheduleRecycleTask(item);
                return;
            }

            // Check if task can be performed on this item
            if (!CanPerformTask(item, taskType))
            {
                Debug.LogWarning($"[WorkshopService] Cannot schedule {taskType} task - item {item.Name} doesn't have required tags or is destroyed");
                return;
            }

            // Calculate ready time based on task type
            var readyAt = CalculateReadyTime(taskType);

            var task = new ProcessingTask
            {
                ProcessingType = taskType,
                Item = item,
                ReadyAt = readyAt
            };

            _scheduledTasks.Add(task);
            Debug.Log($"[WorkshopService] Task scheduled: {taskType} for {item.Name} at {readyAt}");
        }

        private void ScheduleRecycleTask(ItemModel item)
        {
            // Calculate ready time for recycle
            var readyAt = CalculateReadyTime(ProcessingType.Recycle);

            var task = new ProcessingTask
            {
                ProcessingType = ProcessingType.Recycle,
                Item = item,
                ReadyAt = readyAt
            };

            _scheduledTasks.Add(task);
            Debug.Log($"[WorkshopService] Recycle task scheduled for {item.Name} at {readyAt}");
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

        public bool CanPerformTask(ItemModel item, ProcessingType taskType)
        {
            if (item == null) return false;

            // Cannot perform task on destroyed items
            if (IsDestroyed(item)) return false;

            // Check if item has tags with the specified ProcessingType that are revealed to player
            return HasKnownTagsByProcessType(item, taskType);
        }

        private bool HasKnownTagsByProcessType(ItemModel item, ProcessingType taskType)
        {
            return item.Tags.Any(tag =>
                tag.ProcessingType == taskType &&
                tag.IsRevealedToPlayer);
        }

        private BaseTagModel GetRevealedTagWithProcessingType(ItemModel item, ProcessingType taskType)
        {
            return item.Tags.FirstOrDefault(tag =>
                tag.ProcessingType == taskType &&
                tag.IsRevealedToPlayer);
        }

        private GameTime CalculateReadyTime(ProcessingType taskType)
        {
            var currentTime = _timeService.CurrentTime;
            var item = _workshopStorage.Get(0);

            int taskDurationMinutes = 10; // Default duration

            if (item != null)
            {
                var relevantTag = GetRevealedTagWithProcessingType(item, taskType);
                if (relevantTag != null)
                {
                    taskDurationMinutes = relevantTag.ProcessingDurationInMinutes;
                }
            }

            return new GameTime(currentTime.Day, currentTime.Hour, currentTime.Minute + taskDurationMinutes);
        }

        private void OnWorkshopItemChanged(int slotId)
        {
            var item = _workshopStorage.Get(slotId);

            if (item != null && CurrentItem == null)
            {
                // Item was added
                CurrentItem = item;
                OnItemAdded?.Invoke(item);
            }
            else if (item == null && CurrentItem != null)
            {
                // Item was removed
                var removedItem = CurrentItem;
                CurrentItem = null;
                OnItemRemoved?.Invoke(removedItem);
            }
        }

        private void OnTimeChanged(GameTime currentTime)
        {
            for (int i = _scheduledTasks.Count - 1; i >= 0; i--)
            {
                var task = _scheduledTasks[i];
                if (IsTimeReached(task.ReadyAt, currentTime))
                {
                    ProcessTask(task);
                    _scheduledTasks.RemoveAt(i);
                }
            }
        }

        private bool IsTimeReached(GameTime readyAt, GameTime currentTime)
        {
            return currentTime >= readyAt;
        }

        private void ProcessTask(ProcessingTask task)
        {
            ProcessItemByType(task.Item, task.ProcessingType);
            Debug.Log($"[WorkshopService] {task.ProcessingType} completed for {task.Item.Name}");

            // Fire task completed event
            OnTaskCompleted?.Invoke(task);
        }

        private void ProcessItemByType(ItemModel item, ProcessingType processingType)
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

        private void ProcessRecycle(ItemModel item)
        {
            // Break down item into materials
            // Add materials to inventory
            // Remove item from workshop
            Debug.Log($"[WorkshopService] Item {item.Name} recycled - broken down into materials");
        }

        // Tag upgrade/degrade with chance logic (private)
        private bool TryUpgradeTag(ItemModel item, TagType tagType)
        {
            if (item == null) return false;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return false;

            // Check upgrade chance from tag
            float upgradeChance = currentTag.UpgradeChance;
            if (Random.Range(0f, 1f) > upgradeChance)
            {
                Debug.Log($"[WorkshopService] Upgrade failed for {tagType} - chance was {upgradeChance:P}");
                return false; // Upgrade failed
            }

            // Perform guaranteed upgrade
            _tagService.Upgrade(item, tagType);
            Debug.Log($"[WorkshopService] Successfully upgraded {tagType} tag");
            return true; // Upgrade successful
        }

        private bool TryDegradeTag(ItemModel item, TagType tagType)
        {
            if (item == null) return false;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return false;

            // Check degrade chance (25% chance that degrade doesn't happen)
            if (Random.Range(0f, 1f) < DEGRADE_CHANCE)
            {
                Debug.Log($"[WorkshopService] Degrade prevented for {tagType} - {DEGRADE_CHANCE:P} protection");
                return false; // Degrade prevented
            }

            // Perform guaranteed degrade
            _tagService.Degrade(item, tagType);
            Debug.Log($"[WorkshopService] Successfully degraded {tagType} tag");
            return true; // Degrade successful
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

        private void ProcessDefaultTask(ItemModel item, ProcessingType processingType)
        {
            if (item == null) return;

            // Check success chance for default task
            if (Random.Range(0f, 1f) <= DEFAULT_TASK_SUCCESS_CHANCE)
            {
                // Task succeeded - remove tags with this processing type
                item.Tags.RemoveAll(tag => tag.ProcessingType == processingType);
                Debug.Log($"[WorkshopService] {processingType} task succeeded - removed tags");
            }
            else
            {
                // Task failed - degrade condition
                Debug.Log($"[WorkshopService] {processingType} task failed - degrading condition");
                TryDegradeTag(item, TagType.Condition);
            }
        }
    }
}
