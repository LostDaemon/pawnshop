using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemRepositoryService : IItemRepositoryService
{
    private readonly List<ItemPrototype> _items;
    private readonly System.Random _random;

    public ItemRepositoryService()
    {
        _random = new System.Random();
        _items = new List<ItemPrototype>();
    }

    public void Load()
    {
        _items.Clear();
        _items.AddRange(Resources.LoadAll<ItemPrototype>(@"ScriptableObjects\Items").ToList());
        Debug.Log($"Loaded {_items.Count} item prototypes.");
    }

    public ItemModel GetRandomItem()
    {
        int index = _random.Next(_items.Count);
        var itemPrototype = _items[index];
        
        return GetItem(itemPrototype.ClassId);
    }

    public ItemModel GetItem(string classId)
    {
        var itemPrototype = _items.FirstOrDefault(item => item.ClassId == classId);
        if (itemPrototype == null) return null;

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

        // Initialize tags for the item
        InitializeItemTags(result, itemPrototype);

        return result;
    }

    private void InitializeItemTags(ItemModel item, ItemPrototype prototype)
    {
        if (prototype == null) return;

        // Add all required tags first (ignoring probability)
        foreach (var requiredTag in prototype.requiredTags)
        {
            if (requiredTag != null)
            {
                var tagModel = CreateTagModelFromPrototype(requiredTag);
                if (tagModel != null)
                {
                    item.Tags.Add(tagModel);
                }
            }
        }

        // Process available tags based on probability and max count
        foreach (var tagLimit in prototype.allowedTags)
        {
            int maxCount = tagLimit.MaxCount;

            // Check how many tags of this type we already have (including required ones)
            int currentCount = item.Tags.Count(t => t.TagType == tagLimit.TagType);
            int remainingSlots = maxCount - currentCount;

            if (remainingSlots <= 0) continue;

            // Try to add tags based on probability
            for (int i = 0; i < remainingSlots; i++)
            {
                if (Random.Range(0f, 1f) <= GetTagPrototype(tagLimit.TagType)?.AppearanceChance)
                {
                    var tagModel = CreateTagModelFromPrototype(GetTagPrototype(tagLimit.TagType));
                    if (tagModel != null)
                    {
                        item.Tags.Add(tagModel);
                    }
                }
            }
        }
    }

    private BaseTagModel CreateTagModelFromPrototype(BaseTagPrototype prototype)
    {
        if (prototype == null) return null;

        return prototype switch
        {
            SimpleTagPrototype simplePrototype => new SimpleTagModel(simplePrototype),
            TextTagPrototype textPrototype => new TextTagModel(textPrototype),
            NumericTagPrototype numericPrototype => new NumericTagModel(numericPrototype),
            _ => null
        };
    }

    private BaseTagPrototype GetTagPrototype(TagType tagType)
    {
        // This method should be implemented to get the actual prototype
        // For now, returning null - you'll need to implement this based on your architecture
        return null;
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