public interface IItemRepositoryService
{
    ItemModel GetRandomItem();
    public void AddItem(ItemPrototype itemPrototype);
    public void RemoveItem(ItemPrototype itemPrototype);
}