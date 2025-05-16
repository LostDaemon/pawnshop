public class ItemModel
{
    public string Name;
    public string ImageId;
    public int RealPrice;
    public float Scale;
    public string Description;

    public ItemModel(string name, string imageId, int realPrice, float scale, string description)
    {
        Name = name;
        ImageId = imageId;
        RealPrice = realPrice;
        Description = description;
        Scale = scale;
    }
}