using System.Linq;

namespace PawnShop.Services
{
    public class StorageRouterService<T> : IStorageRouterService<T>
    {
        public ISlotStorageService<T> Source { get; private set; }
        public ISlotStorageService<T> Target { get; private set; }
        public T Payload { get; private set; }

        public void SetPayload(T payload)
        {
            Payload = payload;
        }

        public void SetSource(ISlotStorageService<T> source)
        {
            Source = source;
        }

        public void SetTarget(ISlotStorageService<T> target)
        {
            Target = target;
        }

        public void Transfer(T item, ISlotStorageService<T> source, ISlotStorageService<T> target)
        {
            if (!source.HasItem(item))
            {
                UnityEngine.Debug.LogWarning("Item not found in source storage.");
                return;
            }

            if (source.Withdraw(item))
            {
                if (!target.Put(item))
                {
                    // If target storage is full, put item back to source
                    source.Put(item);
                    UnityEngine.Debug.LogWarning("Target storage is full. Item returned to source.");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Failed to withdraw item from source storage.");
            }
        }
    }
}