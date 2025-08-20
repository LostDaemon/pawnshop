using System.Collections.Generic;

public class Customer : BaseCharacter
{
    public ItemModel OwnedItem { get; set; }
    public float UncertaintyLevel { get; set; }
    public float MoodLevel { get; set; }
}