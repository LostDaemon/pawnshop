using System.Collections.Generic;

public class ItemRepositoryService : IItemRepositoryService
{
    private readonly List<ItemPrototype> _items;
    private readonly System.Random _random;

    public ItemRepositoryService()
    {
        _random = new System.Random();

        _items = new List<ItemPrototype>();
    }

    public ItemModel GetRandomItem()
    {
        int index = _random.Next(_items.Count);
        var itemPrototype = _items[index];
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

        return result;
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