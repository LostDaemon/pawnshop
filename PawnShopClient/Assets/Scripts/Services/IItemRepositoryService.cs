public interface IItemRepositoryService
{
    ItemModel GetRandomItem();
    public void Load();
    public void AddItem(ItemPrototype itemPrototype);
    public void RemoveItem(ItemPrototype itemPrototype);
}