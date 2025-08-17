public interface IItemRepositoryService
{
    ItemModel GetRandomItem();
    ItemModel GetItem(string classId);
    public void Load();
    public void AddItem(ItemPrototype itemPrototype);
    public void RemoveItem(ItemPrototype itemPrototype);
}