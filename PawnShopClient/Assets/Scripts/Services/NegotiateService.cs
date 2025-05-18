using System;
using System.Collections.Generic;
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
    public Customer CurrentCustomer { get; private set; }
    public long CurrentNpcOffer { get; private set; }

    private long _agreedOffer;
    private readonly HashSet<long> _rejectedOffers = new();

    private static readonly float[] AllDiscounts = { 0.10f, 0.25f, 0.50f, 0.75f };

    public NegotiateService(
        IWalletService wallet,
        IGameStorageService<ItemModel> inventory)
    {
        _wallet = wallet;
        _inventory = inventory;
    }

    public void SetCurrentCustomer(Customer customer)
    {
        CurrentCustomer = customer;
        CurrentItem = customer.OwnedItem;

        GenerateInitialNpcOffer();
        _agreedOffer = CurrentNpcOffer;
        _rejectedOffers.Clear();

        OnCurrentItemChanged?.Invoke(CurrentItem);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;
        CurrentNpcOffer = (long)(CurrentItem.RealPrice * _random.Next(60, 86) / 100f);
    }

    public long GetCurrentOffer() => _agreedOffer;

    public bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock)
    {
        newOffer = Mathf.RoundToInt(_agreedOffer * (1f - discount));
        accepted = TryCounterOffer(newOffer);
        discountsToBlock = new List<float>();

        if (accepted)
        {
            _agreedOffer = newOffer;
        }
        else
        {
            foreach (var d in AllDiscounts)
            {
                if (d >= discount)
                    discountsToBlock.Add(d);
            }
        }

        return true;
    }

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

    public bool TryCounterOffer(long playerOffer)
    {
        if (CurrentItem == null)
            return false;

        if (_rejectedOffers.Contains(playerOffer))
            return false;

        var minAcceptable = (long)(CurrentItem.RealPrice * 0.6f);
        var maxAcceptable = (long)(CurrentItem.RealPrice * 0.95f);

        if (playerOffer < minAcceptable)
            return false;

        if (playerOffer > maxAcceptable)
            playerOffer = maxAcceptable;

        var range = maxAcceptable - minAcceptable;
        var distance = playerOffer - minAcceptable;
        float t = (float)distance / range;

        float chance = Mathf.Lerp(0.9f, 0.1f, t);
        bool success = _random.NextDouble() < chance;

        if (!success)
            _rejectedOffers.Add(playerOffer);

        return success;
    }

    public void RequestSkip()
    {
        OnSkipRequested?.Invoke();
    }
}