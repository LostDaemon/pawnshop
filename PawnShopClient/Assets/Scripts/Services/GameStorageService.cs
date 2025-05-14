using System.Collections.Generic;

public class GameStorageService<T> : IGameStorageService<T>
{
    private readonly List<T> _items = new();

    public IReadOnlyList<T> All => _items;

    public void Put(T item)
    {
        _items.Add(item);
    }

    public bool Withdraw(T item)
    {
        return _items.Remove(item);
    }

    public void Empty()
    {
        _items.Clear();
    }
}