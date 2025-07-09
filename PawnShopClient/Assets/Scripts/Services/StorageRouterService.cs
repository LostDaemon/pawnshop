using System.Linq;

public class StorageRouterService<T> : IStorageRouterService<T>
{
    public IGameStorageService<T> Source { get; private set; }
    public IGameStorageService<T> Target { get; private set; }
    public T Payload { get; private set; }

    public void SetPayload(T payload)
    {
        Payload = payload;
    }

    public void SetSource(IGameStorageService<T> source)
    {
        Source = source;
    }

    public void SetTarget(IGameStorageService<T> target)
    {
        Target = target;
    }

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