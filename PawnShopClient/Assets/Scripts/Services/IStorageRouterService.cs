namespace PawnShop.Services
{
    public interface IStorageRouterService<T>
    {
        ISlotStorageService<T> Source { get; }
        ISlotStorageService<T> Target { get; }
        T Payload { get; }
        void SetSource(ISlotStorageService<T> source);
        void SetPayload(T payload);
        void SetTarget(ISlotStorageService<T> target);
        void Transfer(T item, ISlotStorageService<T> source, ISlotStorageService<T> target);
    }
}