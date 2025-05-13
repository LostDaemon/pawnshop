using System.Collections.Generic;

public class ItemRepositoryService : IItemRepositoryService
{
    private readonly List<ItemModel> _items;
    private readonly System.Random _random;

    public ItemRepositoryService()
    {
        _random = new System.Random();

        _items = new List<ItemModel>
        {
            new ItemModel("Golden Watch", "item_watch_gold", 500, "Luxury gold-plated watch."),
            new ItemModel("Old Book", "item_book_old", 25, "Worn-out classic literature."),
            new ItemModel("Smartphone", "item_smartphone", 300, "Used but functional device."),
        };
    }

    public ItemModel GetRandomItem()
    {
        int index = _random.Next(_items.Count);
        return _items[index];
    }
}