using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NegotiationService : INegotiationService
{
    private readonly IWalletService _wallet;
    private readonly IGameStorageService<ItemModel> _inventory;
    private readonly IGameStorageService<ItemModel> _sellStorage;
    private readonly ICustomerService _customerService;
    private readonly INegotiationHistoryService _history;
    private readonly System.Random _random = new();

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;
    public event Action OnSkipRequested;

    public ItemModel CurrentItem { get; private set; }

    public long CurrentNpcOffer { get; private set; }
    private long _agreedOffer;
    private readonly HashSet<long> _rejectedOffers = new();
    private static readonly float[] AllDiscounts = { 0.10f, 0.25f, 0.50f, 0.75f };

    [Inject]
    public NegotiationService(
        IWalletService wallet,
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        [Inject(Id = StorageType.SellStorage)] IGameStorageService<ItemModel> sellStorage,
        ICustomerService customerService,
        INegotiationHistoryService history)
    {
        _wallet = wallet;
        _inventory = inventory;
        _sellStorage = sellStorage;
        _customerService = customerService;
        _history = history;
    }

    public void SetCurrentCustomer(Customer customer)
    {
        _customerService.SetCurrent(customer);
        CurrentItem = customer.OwnedItem;

        GenerateInitialNpcOffer();
        _agreedOffer = CurrentNpcOffer;
        _rejectedOffers.Clear();

        _history.Add(new TextRecord("Customer", $"I'd sell '{CurrentItem.Name}' for {CurrentNpcOffer}."));
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
        Debug.Log($"Trying discount: {discount * 100}% => New offer: {newOffer}");

        accepted = TryCounterOffer(newOffer);
        discountsToBlock = new List<float>();

        _history.Add(new TextRecord("Player", $"Offered {newOffer} ({discount * 100}% discount)"));

        if (accepted)
        {
            _agreedOffer = newOffer;
            _history.Add(new TextRecord("Customer", $"Okay, let's do {newOffer}."));
        }
        else
        {
            _history.Add(new TextRecord("Customer", "No way. Too low."));
            _customerService.ChangeMood(-0.25f);

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
        {
            _history.Add(new TextRecord("System", $"Not enough money to buy '{CurrentItem.Name}' for {offeredPrice}."));
            return false;
        }

        _inventory.Put(CurrentItem);
        _history.Add(new TextRecord("System", $"Item '{CurrentItem.Name}' purchased for {offeredPrice}."));
        OnPurchased?.Invoke(CurrentItem);
        return true;
    }

    public bool TryCounterOffer(long playerOffer)
    {
        if (CurrentItem == null)
            return false;

        if (_rejectedOffers.Contains(playerOffer))
            return false;

        var customer = _customerService.Current;

        var minAcceptable = (long)(CurrentItem.RealPrice * 0.6f * (1f - customer.UncertaintyLevel));
        var maxAcceptable = (long)(CurrentItem.RealPrice * 0.95f);

        Debug.Log($"NPC offer: {CurrentNpcOffer}, Player offer: {playerOffer}, Min: {minAcceptable}, Max: {maxAcceptable}");

        if (playerOffer < minAcceptable)
            return false;

        if (playerOffer > maxAcceptable)
            playerOffer = maxAcceptable;

        var range = maxAcceptable - minAcceptable;
        var distance = playerOffer - minAcceptable;
        float t = (float)distance / range;

        float chance = Mathf.Lerp(0.9f, 0.1f, t);
        bool success = _random.NextDouble() < chance;
        Debug.Log($"Counter offer success: {success} (Chance: {chance})");

        if (!success)
            _rejectedOffers.Add(playerOffer);

        return success;
    }

    public void RequestSkip()
    {
        _history.Add(new TextRecord("Player", $"Skipped '{CurrentItem?.Name}'"));
        OnSkipRequested?.Invoke();
    }

    public void AskAboutItemOrigin()
    {
        if (CurrentItem == null || _customerService.Current == null)
            return;

        if (CurrentItem.IsFake)
        {
            _customerService.IncreaseUncertainty(0.25f);
            _history.Add(new TextRecord("Customer", "Umm... I'm not sure where this item came from, to be honest."));
        }
        else
        {
            _history.Add(new TextRecord("Customer", "Of course! It’s been in the family for years."));
        }
    }
}