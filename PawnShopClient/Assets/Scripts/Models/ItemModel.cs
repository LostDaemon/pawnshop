
public class ItemModel
{
    public string Id { get; set; }
    public string ClassId { get; set; }
    public long PurchasePrice { get; set; }
    public long SellPrice { get; set; }
    public bool IsFake { get; set; }
    public int Condition { get; set; }
    public bool Inspected { get; set; } //TODO: Temporary variable. To be replaced by "tags" [{TagType, Revealed}]
    public string Name { get; set; }
    public string ImageId { get; set; }
    public long BasePrice { get; set; }
    public float Scale { get; set; }
    public string Description { get; set; }
}
