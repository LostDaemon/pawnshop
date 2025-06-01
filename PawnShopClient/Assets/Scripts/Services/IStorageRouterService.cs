public interface IStorageRouterService<T>
{
    void Transfer(T item, IGameStorageService<T> source, IGameStorageService<T> target);
}