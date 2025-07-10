
public class ItemModel : ItemPrototypeModel
{
    public string Id { get; private set; }
    public long PurchasePrice { get; set; }
    public long SellPrice { get; set; }
    public bool IsFake { get; set; }

    public ItemModel(string id, bool fake, string name, string imageId, long basePrice, float scale, string description) : base(name, imageId, basePrice, scale, description)
    {
        Id = id;
        IsFake = fake;
    }
}