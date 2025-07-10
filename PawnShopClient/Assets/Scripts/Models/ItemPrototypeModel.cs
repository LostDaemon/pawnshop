
public class ItemPrototypeModel
{
    public string Name { get; private set; }
    public string ImageId { get; private set; }
    public long BasePrice { get; private set; }
    public float Scale { get; private set; }
    public string Description { get; private set; }

    public ItemPrototypeModel(string name, string imageId, long basePrice, float scale, string description)
    {
        Name = name;
        ImageId = imageId;
        BasePrice = basePrice;
        Description = description;
        Scale = scale;
    }
}