using System;
using UnityEngine;

public class NegotiateService : INegotiateService
{
    private readonly IWalletService _wallet;
    private readonly IGameStorageService<ItemModel> _inventory;
    private readonly System.Random _random = new();

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;
    public event Action OnSkipRequested;

    public ItemModel CurrentItem { get; private set; }

    // The initial price offered by the NPC for the current item
    public long CurrentNpcOffer { get; private set; }

    public NegotiateService(IWalletService wallet, IGameStorageService<ItemModel> inventory)
    {
        _wallet = wallet;
        _inventory = inventory;
    }

    public void SetCurrentItem(ItemModel item)
    {
        CurrentItem = item;
        GenerateInitialNpcOffer();
        OnCurrentItemChanged?.Invoke(item);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;

        // Offer between 60% and 85% of real item price
        CurrentNpcOffer = (long)(CurrentItem.RealPrice * _random.Next(60, 86) / 100f);
    }

    /// <summary>
    /// Player accepts the current offer. Tries to finalize the purchase.
    /// </summary>
    public bool TryPurchase(long offeredPrice)
    {
        if (CurrentItem == null)
            return false;

        var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
        if (!success)
            return false;

        _inventory.Put(CurrentItem);
        OnPurchased?.Invoke(CurrentItem);
        return true;
    }

    /// <summary>
    /// Player makes a counter-offer. Returns true if NPC accepts.
    /// </summary>
    public bool TryCounterOffer(long playerOffer)
    {
        if (CurrentItem == null)
            return false;

        var minAcceptable = (long)(CurrentItem.RealPrice * 0.6f);  // минимум
        var maxAcceptable = (long)(CurrentItem.RealPrice * 0.95f); // максимум

        if (playerOffer < minAcceptable || playerOffer > maxAcceptable)
            return false;

        // Вероятность отказа — чем ближе к максимуму, тем выше
        var range = maxAcceptable - minAcceptable;
        var distance = playerOffer - minAcceptable;
        float t = (float)distance / range;

        // Чем ближе к максимуму — тем меньше шанс (0.9 → 0.1)
        float chance = Mathf.Lerp(0.9f, 0.1f, t);
        return _random.NextDouble() < chance;
    }

    public void RequestSkip()
    {
        OnSkipRequested?.Invoke();
    }
}