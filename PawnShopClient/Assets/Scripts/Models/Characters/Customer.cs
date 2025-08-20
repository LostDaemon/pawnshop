public class Customer : BaseCharacter
{
    public ItemModel OwnedItem { get; set; }
    public float UncertaintyLevel { get; set; }
    public float MoodLevel { get; set; }

    public Customer(ItemModel ownedItem, float moodLevel = 0f) : base("Customer")
    {
        OwnedItem = ownedItem;
        UncertaintyLevel = 0f;
        MoodLevel = moodLevel;
    }

    public Customer(string name, ItemModel ownedItem, float moodLevel = 0f) : base(name)
    {
        OwnedItem = ownedItem;
        UncertaintyLevel = 0f;
        MoodLevel = moodLevel;
    }
}