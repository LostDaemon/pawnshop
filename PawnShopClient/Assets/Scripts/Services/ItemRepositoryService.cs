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
    new ItemModel("Golden Watch", "item_watch_gold", 1200, 0.3f, "An elegant gold-plated wristwatch with classic design. Popular among collectors."),
    new ItemModel("Brilliant Ring", "item_brilliant_ring", 2200, 0.3f, "A luxurious ring set with a flawless brilliant-cut diamond. Pure extravagance."),
    new ItemModel("Family Picture", "item_picture", 50, 0.5f, "A faded old photograph in a wooden frame. Sentimental value, little market interest."),
    new ItemModel("Vintage Ink Pen", "item_ink_pen", 250, 0.3f, "A well-preserved ink pen from the 1950s. Might appeal to niche collectors."),
    new ItemModel("Vintage Glasses", "item_vintage_glasses", 180, 0.5f, "Stylish round-frame glasses with gold accents. Slightly scratched."),
    new ItemModel("Golden Ingot", "item_golden_ingot", 3500, 0.5f, "A solid ingot of investment-grade gold. Very high market value."),
    new ItemModel("Old Book", "item_old_book", 120, 0.6f, "A dusty book with worn leather cover. First edition adds slight value."),
    new ItemModel("Brass Hinge", "item_brass_hinge", 25, 0.3f, "An ornate antique hinge. Minor detail piece, limited standalone value."),
    new ItemModel("Unknown Old Mixture", "item_old_mixture", 80, 0.5f, "A sealed bottle with unknown liquid. Possibly alchemical or medical."),
    new ItemModel("Brilliant", "item_brilliant", 1500, 0.3f, "A loose brilliant-cut gemstone. Authentic, but needs professional setting."),
};
    }

    public ItemModel GetRandomItem()
    {
        int index = _random.Next(_items.Count);
        var itemResult = _items[index];
        bool isFake = _random.NextDouble() < 0.25;
        itemResult.IsFake = isFake;
        return _items[index];
    }
}