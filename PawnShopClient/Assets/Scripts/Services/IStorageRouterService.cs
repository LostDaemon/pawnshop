public interface IStorageRouterService
{
    void Transfer(ItemModel item, IGameStorageService<ItemModel> source, IGameStorageService<ItemModel> target);
}