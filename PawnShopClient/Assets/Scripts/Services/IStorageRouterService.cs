public interface IStorageRouterService<T>
{
    IGameStorageService<T> Source { get; }
    IGameStorageService<T> Target { get; }
    T Payload { get; }
    void SetSource(IGameStorageService<T> source);
    void SetPayload(T payload);
    void SetTarget(IGameStorageService<T> target);
    void Transfer(T item, IGameStorageService<T> source, IGameStorageService<T> target);
}