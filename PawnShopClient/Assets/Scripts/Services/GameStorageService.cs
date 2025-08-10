using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStorageService<T> : IGameStorageService<T>
{
    private readonly List<T> _items = new();
    public IReadOnlyList<T> All => _items;

    public event Action<T> OnItemAdded;
    public event Action<T> OnItemRemoved;

    public void Put(T item)
    {
        _items.Add(item);
        OnItemAdded?.Invoke(item);
        Debug.Log($"[Storage] Item added. Total items: {_items.Count}");
    }

    public bool Withdraw(T item)
    {
        bool removed = _items.Remove(item);
        if (removed)
            OnItemRemoved?.Invoke(item);
        return removed;
    }

    public void Empty()
    {
        foreach (var item in _items)
            OnItemRemoved?.Invoke(item);

        _items.Clear();
    }
}