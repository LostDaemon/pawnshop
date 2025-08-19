using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemRepositoryService : IItemRepositoryService
{
    private readonly List<ItemPrototype> _items;
    private readonly System.Random _random;
    private readonly ITagRepositoryService _tagRepository;

    public ItemRepositoryService(ITagRepositoryService tagRepository)
    {
        _random = new System.Random();
        _items = new List<ItemPrototype>();
        _tagRepository = tagRepository;
    }

    public void Load()
    {
        Debug.Log("[ItemRepositoryService] Loading item prototypes...");
        _items.Clear();
        _items.AddRange(Resources.LoadAll<ItemPrototype>(@"ScriptableObjects\Items").ToList());
        Debug.Log($"[ItemRepositoryService] Loaded {_items.Count} item prototypes.");
    }

    public ItemModel GetRandomItem()
    {
        int index = _random.Next(_items.Count);
        var itemPrototype = _items[index];
        
        return GetItem(itemPrototype.ClassId);
    }

    public ItemModel GetItem(string classId)
    {
        Debug.Log($"[ItemRepositoryService] Getting item with classId: {classId}");
        
        var itemPrototype = _items.FirstOrDefault(item => item.ClassId == classId);
        if (itemPrototype == null) 
        {
            Debug.LogWarning($"[ItemRepositoryService] Item prototype not found for classId: {classId}");
            return null;
        }

        Debug.Log($"[ItemRepositoryService] Found item prototype: {itemPrototype.Name}");

        bool isFake = _random.NextDouble() < 0.25;

        var result = new ItemModel()
        {
            Id = System.Guid.NewGuid().ToString(),
            ClassId = itemPrototype.ClassId,
            Name = itemPrototype.Name,
            ImageId = itemPrototype.ImageId,
            BasePrice = itemPrototype.BasePrice,
            IsFake = isFake,
            Scale = itemPrototype.Scale,
            Description = itemPrototype.Description,
            PurchasePrice = 0,
            SellPrice = 0,
            Inspected = false,
            Condition = _random.Next(0, 100)
        };

        Debug.Log($"[ItemRepositoryService] Created ItemModel: {result.Name}, Tags count before initialization: {result.Tags?.Count ?? 0}");

        // Initialize tags for the item
        InitializeItemTags(result, itemPrototype);

        Debug.Log($"[ItemRepositoryService] Final ItemModel: {result.Name}, Tags count: {result.Tags?.Count ?? 0}");

        return result;
    }

    private void InitializeItemTags(ItemModel item, ItemPrototype prototype)
    {
        if (prototype == null) return;

        Debug.Log($"[ItemRepositoryService] Initializing tags for item: {prototype.Name}");
        Debug.Log($"[ItemRepositoryService] Required tags count: {prototype.requiredTags?.Count ?? 0}");
        Debug.Log($"[ItemRepositoryService] Override tags generation: {prototype.OverrideTagsGeneration}");
        Debug.Log($"[ItemRepositoryService] Overrided tags count: {prototype.OverridedTags?.Count ?? 0}");
        Debug.Log($"[ItemRepositoryService] Allowed tags count: {prototype.allowedTags?.Count ?? 0}");

        if (prototype.OverrideTagsGeneration)
        {
            ProcessOverridedTags(item, prototype);
        }
        else
        {
            ProcessRandomTagGeneration(item, prototype);
        }

        Debug.Log($"[ItemRepositoryService] Final tags count for item {item.Name}: {item.Tags.Count}");
    }

    private void ProcessOverridedTags(ItemModel item, ItemPrototype prototype)
    {
        Debug.Log($"[ItemRepositoryService] Using overrided tags for item: {prototype.Name}");
        
        foreach (var overridedTag in prototype.OverridedTags)
        {
            if (overridedTag != null)
            {
                Debug.Log($"[ItemRepositoryService] Processing overrided tag: {overridedTag.TagType}");
                var tagModel = CreateTagModelFromPrototype(overridedTag);
                if (tagModel != null)
                {
                    item.Tags.Add(tagModel);
                    Debug.Log($"[ItemRepositoryService] Added overrided tag: {tagModel.TagType}");
                }
                else
                {
                    Debug.LogWarning($"[ItemRepositoryService] Failed to create tag model for overrided tag: {overridedTag.TagType}");
                }
            }
        }
    }

    private void ProcessRandomTagGeneration(ItemModel item, ItemPrototype prototype)
    {
        Debug.Log($"[ItemRepositoryService] Using random tag generation for item: {prototype.Name}");
        
        // First, add all required tags (ignoring probability)
        Debug.Log($"[ItemRepositoryService] Adding required tags for item: {prototype.Name}");
        foreach (var requiredTagType in prototype.requiredTags)
        {
            Debug.Log($"[ItemRepositoryService] Processing required tag type: {requiredTagType}");
            
            // Get all tags of this type
            var availableTags = _tagRepository.GetTagPrototypesByType(requiredTagType);
            if (availableTags.Count > 0)
            {
                // Select one tag with weighted probability, but one must be selected
                var selectedTag = SelectRequiredTagByProbability(availableTags);
                Debug.Log($"[ItemRepositoryService] Selected required tag with weighted probability: {selectedTag.DisplayName}");
                
                var tagModel = CreateTagModelFromPrototype(selectedTag);
                if (tagModel != null)
                {
                    item.Tags.Add(tagModel);
                    Debug.Log($"[ItemRepositoryService] Added required tag: {tagModel.TagType} - {tagModel.DisplayName}");
                }
                else
                {
                    Debug.LogWarning($"[ItemRepositoryService] Failed to create tag model for required tag type: {requiredTagType}");
                }
            }
            else
            {
                Debug.LogWarning($"[ItemRepositoryService] Failed to get tag prototype for required tag type: {requiredTagType}");
            }
        }
        
        // Then, process available tags based on probability and max count
        Debug.Log($"[ItemRepositoryService] Processing allowed tags for item: {prototype.Name}");
        foreach (var tagLimit in prototype.allowedTags)
        {
            int maxCount = tagLimit.MaxCount;
            Debug.Log($"[ItemRepositoryService] Processing allowed tag type: {tagLimit.TagType}, MaxCount: {maxCount}");

            // Check how many tags of this type we already have (including required ones)
            int currentCount = item.Tags.Count(t => t.TagType == tagLimit.TagType);
            int remainingSlots = maxCount - currentCount;

            if (remainingSlots <= 0) 
            {
                Debug.Log($"[ItemRepositoryService] No remaining slots for tag type: {tagLimit.TagType}");
                continue;
            }

            // Get all available tags of this type
            var availableTags = _tagRepository.GetTagPrototypesByType(tagLimit.TagType);
            if (availableTags.Count == 0)
            {
                Debug.LogWarning($"[ItemRepositoryService] No tag prototypes found for type: {tagLimit.TagType}");
                continue;
            }

            Debug.Log($"[ItemRepositoryService] Found {availableTags.Count} available tags for type {tagLimit.TagType}");

            // Try to add tags based on probability for each remaining slot
            for (int i = 0; i < remainingSlots; i++)
            {
                // Go through all available tags and try to add them based on their AppearanceChance
                foreach (var tag in availableTags)
                {
                    // Check if we already have this specific tag prototype (to avoid duplicates)
                    if (item.Tags.Any(existingTag => existingTag.DisplayName == tag.DisplayName))
                    {
                        Debug.Log($"[ItemRepositoryService] Skipping tag {tag.DisplayName} - already exists");
                        continue;
                    }

                    Debug.Log($"[ItemRepositoryService] Checking tag: {tag.DisplayName} with chance: {tag.AppearanceChance}");
                    if (Random.Range(0f, 1f) <= tag.AppearanceChance)
                    {
                        var tagModel = CreateTagModelFromPrototype(tag);
                        if (tagModel != null)
                        {
                            item.Tags.Add(tagModel);
                            Debug.Log($"[ItemRepositoryService] Added allowed tag: {tagModel.TagType} - {tagModel.DisplayName}");
                            break; // Move to next slot
                        }
                        else
                        {
                            Debug.LogWarning($"[ItemRepositoryService] Failed to create tag model for allowed tag: {tag.TagType}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[ItemRepositoryService] Tag {tag.TagType} failed probability check");
                    }
                }
            }
        }
    }

    private BaseTagModel CreateTagModelFromPrototype(BaseTagPrototype prototype)
    {
        if (prototype == null) return null;

        Debug.Log($"[ItemRepositoryService] Creating tag model from prototype: {prototype.TagType}, Type: {prototype.GetType().Name}");

        BaseTagModel result = prototype switch
        {
            SimpleTagPrototype simplePrototype => new SimpleTagModel(simplePrototype),
            TextTagPrototype textPrototype => new TextTagModel(textPrototype),
            NumericTagPrototype numericPrototype => new NumericTagModel(numericPrototype),
            _ => null
        };

        if (result != null)
        {
            Debug.Log($"[ItemRepositoryService] Successfully created tag model: {result.TagType}, DisplayName: {result.DisplayName}");
        }
        else
        {
            Debug.LogWarning($"[ItemRepositoryService] Failed to create tag model for prototype type: {prototype.GetType().Name}");
        }

        return result;
    }

    private BaseTagPrototype GetTagPrototype(TagType tagType)
    {
        return _tagRepository?.GetTagPrototype(tagType);
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