public class Customer
{
    public ItemModel OwnedItem { get; set; }
    public float UncertaintyLevel { get; set; }
    public float MoodLevel { get; set; }

    public Customer(ItemModel ownedItem, float moodLevel = 0f)
    {
        OwnedItem = ownedItem;
        UncertaintyLevel = 0f;
        MoodLevel = moodLevel;
    }
}