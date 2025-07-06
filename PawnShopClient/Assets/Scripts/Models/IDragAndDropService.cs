public interface IDragAndDropService
{
    void Drop(IGameStorageService<ItemModel> target);
    void StartDrag(IGameStorageService<ItemModel> source, ItemModel payload);
}