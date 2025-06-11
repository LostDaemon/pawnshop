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
    private readonly ITimeService _timeService;
    private readonly IWalletService _wallet;
    private readonly Dictionary<ItemModel, GameTime> _scheduledSales = new();
    private int _maxSlots;

    public int MaxSlots => _maxSlots;
    public IReadOnlyList<ItemModel> DisplayedItems => _displayed;
    public IReadOnlyDictionary<ItemModel, GameTime> ScheduledSales => _scheduledSales;
    public event Action OnDisplayUpdated;
    public event Action<ItemModel> OnSold;

    [Inject]
    public SellService(
        [Inject(Id = "SellStorage")] IGameStorageService<ItemModel> sellStorage,
        ITimeService timeService,
        IWalletService walletService)
    {
        _sellStorage = sellStorage;
        _timeService = timeService;
        _wallet = walletService;
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

            int delayHours = UnityEngine.Random.Range(1, 4);
            var scheduleTime = AddHours(_timeService.CurrentTime, delayHours);
            _scheduledSales[item] = scheduleTime;
            _timeService.Schedule(scheduleTime, () => SellItem(item));
            UnityEngine.Debug.Log($"Item {item.Name} scheduled for sale at {scheduleTime.Day} {scheduleTime.Hour}:{scheduleTime.Minute}");
        }
    }

    public bool SellItem(ItemModel item)
    {
        if (!_displayed.Contains(item))
            return false;

        _displayed.Remove(item);
        _scheduledSales.Remove(item);
        _wallet.TransactionAttempt(CurrencyType.Money, item.RealPrice);
        OnDisplayUpdated?.Invoke();
        OnSold?.Invoke(item);
        UnityEngine.Debug.Log($"Item {item.Name} sold for {item.RealPrice} money.");
        return true;
    }

    public bool RemoveFromDisplay(ItemModel item)
    {
        bool removed = _displayed.Remove(item);
        if (removed)
        {
            _scheduledSales.Remove(item);
            OnDisplayUpdated?.Invoke();
        }
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

    private GameTime AddHours(GameTime time, int hours)
    {
        const int MinutesInDay = 24 * 60;
        int totalMinutes = time.Hour * 60 + time.Minute + hours * 60;
        int newDay = time.Day + totalMinutes / MinutesInDay;
        totalMinutes %= MinutesInDay;

        return new GameTime(newDay, totalMinutes / 60, totalMinutes % 60);
    }
}