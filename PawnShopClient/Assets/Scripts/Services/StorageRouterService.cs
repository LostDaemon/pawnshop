using System.Linq;

public class StorageRouterService : IStorageRouterService
{
    public void Transfer(ItemModel item, IGameStorageService<ItemModel> source, IGameStorageService<ItemModel> target)
    {
        if (!source.All.Contains(item))
        {
            UnityEngine.Debug.LogWarning("Item not found in source storage.");
            return;
        }

        if (source.Withdraw(item))
        {
            target.Put(item);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Failed to withdraw item from source storage.");
        }
    }
}