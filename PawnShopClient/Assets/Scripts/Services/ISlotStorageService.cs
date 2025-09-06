using System;
using System.Collections.Generic;

namespace PawnShop.Services
{
    public interface ISlotStorageService { }

    public interface ISlotStorageService<T> : ISlotStorageService
    {
        void Put(int slot, T item);
        bool Put(T item);
        bool Withdraw(int slot);
        bool Withdraw(T item);
        T Get(int slot);
        bool HasItem(int slot);
        bool HasItem(T item);
        int GetFreeSlotsCount();
        int GetOccupiedSlotsCount();
        int GetTotalSlotsCount();
        IReadOnlyDictionary<int, T> All { get; }
        void Empty();
        void AddSlots(int count);

        event Action<int, T> OnItemAdded;
        event Action<int, T> OnItemRemoved;
        event Action<int> OnItemChanged;
    }
}
