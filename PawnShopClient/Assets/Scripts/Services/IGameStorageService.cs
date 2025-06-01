using System;
using System.Collections.Generic;

public interface IGameStorageService<T>
{
    void Put(T item);
    bool Withdraw(T item);
    IReadOnlyList<T> All { get; }
    void Empty();

    event Action<T> OnItemAdded;
    event Action<T> OnItemRemoved;
}