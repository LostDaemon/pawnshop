using NUnit.Framework;

public class ItemModel
{
    public string Name { get; private set; }
    public string ImageId { get; private set; }
    public int RealPrice { get; private set; }
    public float Scale { get; private set; }
    public string Description { get; private set; }
    public bool IsFake { get; set; }

    public ItemModel(string name, string imageId, int realPrice, float scale, string description, bool isFake = false)
    {
        Name = name;
        ImageId = imageId;
        RealPrice = realPrice;
        Description = description;
        Scale = scale;
        IsFake = isFake;
    }
}