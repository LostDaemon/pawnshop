using System.Linq;

public class StorageRouterService<T> : IStorageRouterService<T>
{
    public void Transfer(T item, IGameStorageService<T> source, IGameStorageService<T> target)
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