using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public abstract class BaseProcessingService
    {
        protected const float DEGRADE_CHANCE = 0.25f; // 25% chance that degrade doesn't happen
        protected const float DEFAULT_TASK_SUCCESS_CHANCE = 0.8f; // 80% chance for default tasks to succeed

        protected readonly ISlotStorageService<ItemModel> _storage;
        protected readonly IStorageLocatorService _storageLocatorService;
        protected readonly ITimeService _timeService;
        protected readonly ITagService _tagService;
        protected readonly List<ProcessingTask> _scheduledTasks = new List<ProcessingTask>();

        public ItemModel CurrentItem { get; private set; }
        public event System.Action<ItemModel> OnItemAdded;
        public event System.Action<ItemModel> OnItemRemoved;
        public event System.Action<ProcessingTask> OnTaskCompleted;

        protected BaseProcessingService(IStorageLocatorService storageLocatorService, ITimeService timeService, ITagService tagService, StorageType storageType)
        {
            _storageLocatorService = storageLocatorService;
            _timeService = timeService;
            _tagService = tagService;
            _storage = _storageLocatorService.Get(storageType);

            // Subscribe to storage changes
            _storage.OnItemChanged += OnStorageItemChanged;

            // Subscribe to time changes
            _timeService.OnTimeChanged += OnTimeChanged;
        }

        public void ScheduleTask(ProcessingType taskType)
        {
            var item = _storage.Get(0); // Get item from slot 0

            if (item == null)
            {
                Debug.LogWarning($"[{GetServiceName()}] Cannot schedule task - no item in {GetStorageName()} slot 0");
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
                Debug.LogWarning($"[{GetServiceName()}] Cannot schedule {taskType} task - item {item.Name} doesn't have required tags or is destroyed");
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
            Debug.Log($"[{GetServiceName()}] Task scheduled: {taskType} for {item.Name} at {readyAt}");
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
            Debug.Log($"[{GetServiceName()}] Recycle task scheduled for {item.Name} at {readyAt}");
        }

        public virtual bool CanPerformTask(ItemModel item, ProcessingType taskType)
        {
            // Default implementation - can be overridden by derived classes
            return item != null;
        }

        protected bool HasKnownTagsByProcessType(ItemModel item, ProcessingType taskType)
        {
            return item.Tags.Any(tag =>
                tag.ProcessingType == taskType &&
                tag.IsRevealedToPlayer);
        }

        protected BaseTagModel GetRevealedTagWithProcessingType(ItemModel item, ProcessingType taskType)
        {
            return item.Tags.FirstOrDefault(tag =>
                tag.ProcessingType == taskType &&
                tag.IsRevealedToPlayer);
        }

        protected virtual GameTime CalculateReadyTime(ProcessingType taskType)
        {
            var currentTime = _timeService.CurrentTime;
            var item = _storage.Get(0);

            int taskDurationMinutes = 10; // Default duration

            if (item != null)
            {
                var relevantTag = GetRevealedTagWithProcessingType(item, taskType);
                if (relevantTag != null)
                {
                    taskDurationMinutes = relevantTag.ProcessingDurationInMinutes;
                }
            }

            return currentTime + taskDurationMinutes;
        }

        protected virtual void OnStorageItemChanged(int slotId)
        {
            var item = _storage.Get(slotId);

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
            Debug.Log($"[{GetServiceName()}] {task.ProcessingType} completed for {task.Item.Name}");

            // Fire task completed event
            OnTaskCompleted?.Invoke(task);
        }

        protected abstract void ProcessItemByType(ItemModel item, ProcessingType processingType);

        // Tag upgrade/degrade with chance logic (protected)
        protected bool TryUpgradeTag(ItemModel item, TagType tagType)
        {
            if (item == null) return false;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return false;

            // Check upgrade chance from tag
            float upgradeChance = currentTag.UpgradeChance;
            if (Random.Range(0f, 1f) > upgradeChance)
            {
                Debug.Log($"[{GetServiceName()}] Upgrade failed for {tagType} - chance was {upgradeChance:P}");
                return false; // Upgrade failed
            }

            // Perform guaranteed upgrade
            _tagService.Upgrade(item, tagType);
            Debug.Log($"[{GetServiceName()}] Successfully upgraded {tagType} tag");
            return true; // Upgrade successful
        }

        protected bool TryDegradeTag(ItemModel item, TagType tagType)
        {
            if (item == null) return false;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return false;

            // Check degrade chance (25% chance that degrade doesn't happen)
            if (Random.Range(0f, 1f) < DEGRADE_CHANCE)
            {
                Debug.Log($"[{GetServiceName()}] Degrade prevented for {tagType} - {DEGRADE_CHANCE:P} protection");
                return false; // Degrade prevented
            }

            // Perform guaranteed degrade
            _tagService.Degrade(item, tagType);
            Debug.Log($"[{GetServiceName()}] Successfully degraded {tagType} tag");
            return true; // Degrade successful
        }

        protected void ProcessDefaultTask(ItemModel item, ProcessingType processingType)
        {
            if (item == null) return;

            // Check success chance for default task
            if (Random.Range(0f, 1f) <= DEFAULT_TASK_SUCCESS_CHANCE)
            {
                // Task succeeded - remove tags with this processing type
                item.Tags.RemoveAll(tag => tag.ProcessingType == processingType);
                Debug.Log($"[{GetServiceName()}] {processingType} task succeeded - removed tags");
            }
            else
            {
                // Task failed - degrade condition
                Debug.Log($"[{GetServiceName()}] {processingType} task failed - degrading condition");
                TryDegradeTag(item, TagType.Condition);
            }
        }

        // Abstract methods to be implemented by derived classes
        protected abstract string GetServiceName();
        protected abstract string GetStorageName();
    }
}
