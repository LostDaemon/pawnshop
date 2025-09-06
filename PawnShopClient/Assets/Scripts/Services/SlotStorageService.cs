using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PawnShop.Services
{
    public class SlotStorageService<T> : ISlotStorageService<T>
    {
        private readonly Dictionary<int, T> _items = new();
        private int _maxSize = 0;

        public IReadOnlyDictionary<int, T> All => _items;

        public event Action<int, T> OnItemAdded;
        public event Action<int, T> OnItemRemoved;
        public event Action<int> OnItemChanged;

        public void Put(int slot, T item)
        {
            if (slot >= _maxSize)
            {
                Debug.LogError($"[SlotStorage] Slot {slot} is out of bounds. Max size: {_maxSize}");
                return;
            }

            if (_items.ContainsKey(slot) && _items[slot] != null)
            {
                Debug.LogWarning($"[SlotStorage] Slot {slot} is already occupied. Replacing item.");
                OnItemRemoved?.Invoke(slot, _items[slot]);
            }

            _items[slot] = item;

            // Log subscribers count
            var subscriberCount = OnItemAdded?.GetInvocationList()?.Length ?? 0;
            Debug.Log($"[SlotStorage] Put called for slot {slot}. Subscribers to OnItemAdded: {subscriberCount}");

            OnItemAdded?.Invoke(slot, item);
            OnItemChanged?.Invoke(slot);
        }

        public bool Put(T item)
        {
            // Find first empty slot
            for (int i = 0; i < _maxSize; i++)
            {
                if (!_items.ContainsKey(i) || _items[i] == null)
                {
                    Put(i, item);
                    return true;
                }
            }

            Debug.LogWarning($"[SlotStorage] No free slots available. Cannot put item.");
            return false;
        }

        public bool Withdraw(int slot)
        {
            if (_items.TryGetValue(slot, out var item) && item != null)
            {
                _items[slot] = default(T);
                OnItemRemoved?.Invoke(slot, item);
                OnItemChanged?.Invoke(slot);
                return true;
            }
            return false;
        }

        public bool Withdraw(T item)
        {
            var slotToRemove = _items.FirstOrDefault(kvp => EqualityComparer<T>.Default.Equals(kvp.Value, item));
            if (!slotToRemove.Equals(default(KeyValuePair<int, T>)))
            {
                _items[slotToRemove.Key] = default(T);
                OnItemRemoved?.Invoke(slotToRemove.Key, item);
                OnItemChanged?.Invoke(slotToRemove.Key);
                return true;
            }
            return false;
        }

        public T Get(int slot)
        {
            return _items.TryGetValue(slot, out var item) ? item : default(T);
        }

        public bool HasItem(int slot)
        {
            return _items.ContainsKey(slot) && _items[slot] != null;
        }

        public bool HasItem(T item)
        {
            return _items.Values.Contains(item);
        }

        public int GetFreeSlotsCount()
        {
            int freeCount = 0;
            for (int i = 0; i < _maxSize; i++)
            {
                if (!_items.ContainsKey(i) || _items[i] == null)
                {
                    freeCount++;
                }
            }
            return freeCount;
        }

        public int GetOccupiedSlotsCount()
        {
            int occupiedCount = 0;
            for (int i = 0; i < _maxSize; i++)
            {
                if (_items.ContainsKey(i) && _items[i] != null)
                {
                    occupiedCount++;
                }
            }
            return occupiedCount;
        }

        public int GetTotalSlotsCount()
        {
            return _maxSize;
        }

        public void Empty()
        {
            foreach (var kvp in _items)
            {
                if (kvp.Value != null)
                {
                    OnItemRemoved?.Invoke(kvp.Key, kvp.Value);
                    OnItemChanged?.Invoke(kvp.Key);
                }
                _items[kvp.Key] = default(T);
            }
        }

        public void AddSlots(int count)
        {
            if (count < 0)
            {
                Debug.LogError($"[SlotStorage] Invalid count: {count}. Count must be non-negative.");
                return;
            }

            // If dictionary is empty, create all slots with null values
            if (_items.Count == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _items[i] = default(T);
                }
                _maxSize = count;
                Debug.Log($"[SlotStorage] Created {count} empty slots.");
            }
            else
            {
                // Add new empty slots
                for (int i = _maxSize; i < _maxSize + count; i++)
                {
                    _items[i] = default(T);
                }
                _maxSize += count;
                Debug.Log($"[SlotStorage] Added {count} new empty slots. Total slots: {_maxSize}");
            }
        }
    }
}
