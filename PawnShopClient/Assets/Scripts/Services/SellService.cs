using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zenject;
using UnityEngine;

public class SellService : ISellService
{
    private readonly List<ItemModel> _displayed = new();
    private readonly IGameStorageService<ItemModel> _sellStorage;
    private int _maxSlots;

    public int MaxSlots => _maxSlots;
    public IReadOnlyList<ItemModel> DisplayedItems => _displayed;

    public event Action OnDisplayUpdated;
    public event Action<ItemModel> OnSold;

    [Inject]
    public SellService([Inject(Id = "SellStorage")] IGameStorageService<ItemModel> sellStorage)
    {
        _sellStorage = sellStorage;
        _sellStorage.OnItemAdded += HandleNewItemForSale;
    }

    public void ConfigureSlots(int count)
    {
        _maxSlots = count;
    }

    private void HandleNewItemForSale(ItemModel item)
    {
        UnityEngine.Debug.Log($"Trying publish: {item.Name}");
        if (_displayed.Count < _maxSlots && _sellStorage.Withdraw(item))
        {
            _displayed.Add(item);
            OnDisplayUpdated?.Invoke();
        }
    }

    public bool SellItem(ItemModel item)
    {
        if (!_displayed.Contains(item))
            return false;

        _displayed.Remove(item);
        OnDisplayUpdated?.Invoke();
        OnSold?.Invoke(item);

        return true;
    }

    public bool RemoveFromDisplay(ItemModel item)
    {
        bool removed = _displayed.Remove(item);
        if (removed)
            OnDisplayUpdated?.Invoke();
        return removed;
    }

    public void TryAutoFillDisplay()
    {
        if (_displayed.Count >= _maxSlots)
            return;

        foreach (var item in _sellStorage.All.ToList())
        {
            if (_displayed.Count >= _maxSlots)
                break;

            if (_sellStorage.Withdraw(item))
                _displayed.Add(item);
        }

        OnDisplayUpdated?.Invoke();
    }
}