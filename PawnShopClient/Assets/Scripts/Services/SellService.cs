using System;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using Zenject;

namespace PawnShop.Services
{
    public class SellService : ISellService
    {
        private readonly ISlotStorageService<ItemModel> _sellStorage;
        private readonly ISlotStorageService<ItemModel> _inventoryStorage;
        private readonly ITimeService _timeService;
        private readonly IWalletService _wallet;
        private readonly Dictionary<ItemModel, GameTime> _scheduledSales = new();
        private int _maxSlots;

        public int MaxSlots => _maxSlots;
        public IReadOnlyList<ItemModel> DisplayedItems => _sellStorage.All.Values.Where(item => item != null).ToList();
        public IReadOnlyDictionary<ItemModel, GameTime> ScheduledSales => _scheduledSales;
        public event Action<ItemModel> OnStartSelling;
        public event Action<ItemModel> OnSold;

        [Inject]
        public SellService(
            [Inject(Id = StorageType.InventoryStorage)] ISlotStorageService<ItemModel> inventoryStorage,
            [Inject(Id = StorageType.SellStorage)] ISlotStorageService<ItemModel> sellStorage,
            ITimeService timeService,
            IWalletService walletService, int initialSlots)
        {
            _inventoryStorage = inventoryStorage;
            _sellStorage = sellStorage;
            _timeService = timeService;
            _wallet = walletService;
            _maxSlots = initialSlots;
        }

        public void ConfigureSlots(int count)
        {
            _maxSlots = count;
        }

        public bool ScheduleForSale(ItemModel item)
        {
            if (item == null)
                return false;

            if (_sellStorage.GetOccupiedSlotsCount() >= _maxSlots)
            {
                UnityEngine.Debug.LogWarning($"Cannot add {item.Name} with id {item.Id} to sell storage, max slots reached.");
                return false;
            }

            if (!_inventoryStorage.Withdraw(item))
                return false;

            if (!_sellStorage.Put(item))
                return false;

            int delayHours = UnityEngine.Random.Range(1, 4);
            var scheduleTime = AddHours(_timeService.CurrentTime, delayHours);
            _timeService.Schedule(scheduleTime, () => SellScheduledItem(item));
            UnityEngine.Debug.Log($"Item {item.Name} with id {item.Id} scheduled for sale at {scheduleTime.Day} {scheduleTime.Hour}:{scheduleTime.Minute}");

            OnStartSelling?.Invoke(item);
            return true;
        }

        private bool SellScheduledItem(ItemModel item)
        {
            UnityEngine.Debug.Log($"Trying to fire shcheduled sale for {item.Name} with id {item.Id} at {_timeService.CurrentTime.Day} {_timeService.CurrentTime.Hour}:{_timeService.CurrentTime.Minute}");

            if (item == null || !_sellStorage.HasItem(item))
                return false;

            _sellStorage.Withdraw(item);
            _wallet.TransactionAttempt(CurrencyType.Money, item.SellPrice);
            _scheduledSales.Remove(item);
            OnSold?.Invoke(item);
            UnityEngine.Debug.Log($"Item {item.Name} with id {item.Id} sold for {item.SellPrice} at {_timeService.CurrentTime.Day} {_timeService.CurrentTime.Hour}:{_timeService.CurrentTime.Minute}");
            return true;
        }

        public bool RemoveFromSelling(ItemModel item)
        {
            if (item == null || !_sellStorage.HasItem(item))
                return false;

            if (!_sellStorage.Withdraw(item))
                return false;

            _inventoryStorage.Put(item);
            _scheduledSales.Remove(item);
            UnityEngine.Debug.Log($"Item {item.Name} with id {item.Id} removed from selling.");
            return true;
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
}