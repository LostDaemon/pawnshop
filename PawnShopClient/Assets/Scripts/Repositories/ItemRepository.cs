using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects;
using PawnShop.ScriptableObjects.Tags;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly List<ItemPrototype> _items;
        private readonly System.Random _random;
        private readonly ITagService _tagService;

        public ItemRepository(ITagService tagService)
        {
            _random = new System.Random();
            _items = new List<ItemPrototype>();
            _tagService = tagService;
        }

        public void Load()
        {
            Debug.Log("[ItemRepository] Loading item prototypes...");
            _items.Clear();
            _items.AddRange(Resources.LoadAll<ItemPrototype>(@"ScriptableObjects\Items").ToList());
            Debug.Log($"[ItemRepository] Loaded {_items.Count} item prototypes.");
        }

        public ItemModel GetRandomItem()
        {
            int index = _random.Next(_items.Count);
            var itemPrototype = _items[index];

            return GetItem(itemPrototype.ClassId);
        }

        public ItemModel GetItem(string classId)
        {
            Debug.Log($"[ItemRepository] Getting item with classId: {classId}");

            var itemPrototype = _items.FirstOrDefault(item => item.ClassId == classId);
            if (itemPrototype == null)
            {
                Debug.LogWarning($"[ItemRepository] Item prototype not found for classId: {classId}");
                return null;
            }

            Debug.Log($"[ItemRepository] Found item prototype: {itemPrototype.Name}");

            bool isFake = _random.NextDouble() < 0.25;

            var result = new ItemModel()
            {
                Id = System.Guid.NewGuid().ToString(),
                ClassId = itemPrototype.ClassId,
                Name = itemPrototype.Name,
                Image = itemPrototype.Image,
                BasePrice = itemPrototype.BasePrice,
                IsFake = isFake,
                Scale = itemPrototype.Scale,
                Description = itemPrototype.Description,
                PurchasePrice = 0,
                SellPrice = 0,
                Inspected = false,
                Condition = _random.Next(0, 100),
                Materials = new List<MaterialComponent>(itemPrototype.Materials)
            };

            Debug.Log($"[ItemRepository] Created ItemModel: {result.Name}, Tags count before initialization: {result.Tags?.Count ?? 0}");

            // Initialize tags for the item
            InitializeItemTags(result, itemPrototype);

            Debug.Log($"[ItemRepository] Final ItemModel: {result.Name}, Tags count: {result.Tags?.Count ?? 0}");

            return result;
        }

        private void InitializeItemTags(ItemModel item, ItemPrototype prototype)
        {
            if (prototype == null) return;

            Debug.Log($"[ItemRepository] Initializing tags for item: {prototype.Name}");
            Debug.Log($"[ItemRepository] Required tags count: {prototype.requiredTags?.Count ?? 0}");
            Debug.Log($"[ItemRepository] Override tags generation: {prototype.OverrideTagsGeneration}");
            Debug.Log($"[ItemRepository] Overrided tags count: {prototype.OverridedTags?.Count ?? 0}");
            Debug.Log($"[ItemRepository] Allowed tags count: {prototype.allowedTags?.Count ?? 0}");

            if (prototype.OverrideTagsGeneration)
            {
                ProcessOverridedTags(item, prototype);
            }
            else
            {
                ProcessRandomTagGeneration(item, prototype);
            }

            Debug.Log($"[ItemRepository] Final tags count for item {item.Name}: {item.Tags.Count}");
        }

        private void ProcessOverridedTags(ItemModel item, ItemPrototype prototype)
        {
            Debug.Log($"[ItemRepository] Using overrided tags for item: {prototype.Name}");

            foreach (var overridedTag in prototype.OverridedTags)
            {
                if (overridedTag != null)
                {
                    Debug.Log($"[ItemRepository] Processing overrided tag: {overridedTag.TagType}");
                    var tagModel = CreateTagModelFromPrototype(overridedTag);
                    if (tagModel != null)
                    {
                        item.Tags.Add(tagModel);
                        Debug.Log($"[ItemRepository] Added overrided tag: {tagModel.TagType}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ItemRepository] Failed to create tag model for overrided tag: {overridedTag.TagType}");
                    }
                }
            }
        }

        private void ProcessRandomTagGeneration(ItemModel item, ItemPrototype prototype)
        {
            Debug.Log($"[ItemRepository] Using random tag generation for item: {prototype.Name}");

            // First, add all required tags (ignoring probability)
            Debug.Log($"[ItemRepository] Adding required tags for item: {prototype.Name}");
            foreach (var requiredTagType in prototype.requiredTags)
            {
                Debug.Log($"[ItemRepository] Processing required tag type: {requiredTagType}");

                // Get all tags of this type
                var availableTags = _tagService.GetTagPrototypesByType(requiredTagType);
                if (availableTags.Count > 0)
                {
                    // Select one tag with weighted probability, but one must be selected
                    var selectedTag = SelectRequiredTagByProbability(availableTags);
                    Debug.Log($"[ItemRepository] Selected required tag with weighted probability: {selectedTag.DisplayName}");

                    var tagModel = CreateTagModelFromPrototype(selectedTag);
                    if (tagModel != null)
                    {
                        item.Tags.Add(tagModel);
                        Debug.Log($"[ItemRepository] Added required tag: {tagModel.TagType} - {tagModel.DisplayName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ItemRepository] Failed to create tag model for required tag type: {requiredTagType}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ItemRepository] Failed to get tag prototype for required tag type: {requiredTagType}");
                }
            }

            // Then, process available tags based on probability and max count
            foreach (var tagLimit in prototype.allowedTags)
            {
                int maxCount = tagLimit.MaxCount;

                // Check how many tags of this type we already have (including required ones)
                int currentCount = item.Tags.Count(t => t.TagType == tagLimit.TagType);
                int remainingSlots = maxCount - currentCount;

                if (remainingSlots <= 0)
                {
                    continue;
                }

                // Get all available tags of this type
                var availableTags = _tagService.GetTagPrototypesByType(tagLimit.TagType);
                if (availableTags.Count == 0)
                {
                    continue;
                }

                // Try to add tags based on probability for each remaining slot
                for (int i = 0; i < remainingSlots; i++)
                {
                    // Go through all available tags and try to add them based on their AppearanceChance
                    foreach (var tag in availableTags)
                    {
                        // Check if we already have this specific tag prototype (to avoid duplicates)
                        if (item.Tags.Any(existingTag => existingTag.DisplayName == tag.DisplayName))
                        {
                            continue;
                        }

                        if (Random.Range(0f, 1f) <= tag.AppearanceChance)
                        {
                            var tagModel = CreateTagModelFromPrototype(tag);
                            if (tagModel != null)
                            {
                                item.Tags.Add(tagModel);
                                break; // Move to next slot
                            }
                        }
                    }
                }
            }
        }

        private BaseTagModel CreateTagModelFromPrototype(BaseTagPrototype prototype)
        {
            if (prototype == null) return null;

            Debug.Log($"[ItemRepository] Creating tag model from prototype: {prototype.TagType}, Type: {prototype.GetType().Name}");

            BaseTagModel result = _tagService.GetNewTag(prototype);

            if (result != null)
            {
                Debug.Log($"[ItemRepository] Successfully created tag model: {result.TagType}, DisplayName: {result.DisplayName}");
            }
            else
            {
                Debug.LogWarning($"[ItemRepository] Failed to create tag model for prototype type: {prototype.GetType().Name}");
            }

            return result;
        }

        private BaseTagPrototype SelectRequiredTagByProbability(IReadOnlyCollection<BaseTagPrototype> availableTags)
        {
            if (availableTags == null || availableTags.Count == 0) return null;

            // Calculate total weight (sum of all AppearanceChance values)
            float totalWeight = availableTags.Sum(tag => tag.AppearanceChance);

            if (totalWeight <= 0)
            {
                // If all AppearanceChance are 0, select random
                int randomIndex = Random.Range(0, availableTags.Count);
                return availableTags.ElementAt(randomIndex);
            }

            // Select based on weighted probability
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var tag in availableTags)
            {
                currentWeight += tag.AppearanceChance;
                if (randomValue <= currentWeight)
                {
                    return tag;
                }
            }

            // Fallback to last tag (shouldn't happen, but just in case)
            return availableTags.Last();
        }

        public void AddItem(ItemPrototype itemPrototype)
        {
            if (itemPrototype != null && !_items.Contains(itemPrototype))
            {
                _items.Add(itemPrototype);
            }
        }

        public void RemoveItem(ItemPrototype itemPrototype)
        {
            if (itemPrototype != null)
            {
                _items.Remove(itemPrototype);
            }
        }
    }
}
