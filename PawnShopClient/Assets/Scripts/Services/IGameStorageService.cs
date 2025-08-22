using System;
using System.Collections.Generic;

namespace PawnShop.Services
{
    public interface IGameStorageService { }

    public interface IGameStorageService<T> : IGameStorageService
    {
        void Put(T item);
        bool Withdraw(T item);
        IReadOnlyList<T> All { get; }
        void Empty();

        event Action<T> OnItemAdded;
        event Action<T> OnItemRemoved;
    }
}