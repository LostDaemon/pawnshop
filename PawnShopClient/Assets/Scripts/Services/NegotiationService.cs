using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NegotiationService : INegotiationService
{
    private readonly IWalletService _wallet;
    private readonly IGameStorageService<ItemModel> _inventory;

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

    [Inject]
    public NegotiationService(
        IWalletService wallet,
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        ICustomerService customerService,
        INegotiationHistoryService history)
    {
        _wallet = wallet;
        _inventory = inventory;
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

        _history.Add(new TextRecord(HistoryRecordSource.Customer, $"I'd sell '{CurrentItem.Name}' for {CurrentNpcOffer}."));
        OnCurrentItemChanged?.Invoke(CurrentItem);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;
        CurrentNpcOffer = (long)(CurrentItem.BasePrice * _random.Next(60, 86) / 100f);
    }

    public long GetCurrentOffer() => _agreedOffer;



    public bool TryPurchase(long offeredPrice)
    {
        if (CurrentItem == null)
            return false;

        var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
        if (!success)
        {
            _history.Add(new TextRecord(HistoryRecordSource.Customer, "You sure you can pay that?"));
            return false;
        }

        CurrentItem.PurchasePrice = offeredPrice;
        _inventory.Put(CurrentItem);
        _history.Add(new TextRecord(HistoryRecordSource.Customer, $"Deal. {offeredPrice} it is."));
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

        var minAcceptable = (long)(CurrentItem.BasePrice * 0.6f * (1f - customer.UncertaintyLevel));
        var maxAcceptable = (long)(CurrentItem.BasePrice * 0.95f);

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
        _history.Add(new TextRecord(HistoryRecordSource.Player, $"Skipped '{CurrentItem?.Name}'"));
        OnSkipRequested?.Invoke();
    }

    public void AskAboutItemOrigin()
    {
        if (CurrentItem == null || _customerService.Current == null)
            return;

        if (CurrentItem.IsFake)
        {
            _customerService.IncreaseUncertainty(0.25f);
            _history.Add(new TextRecord(HistoryRecordSource.Customer, "Umm... I'm not sure where this item came from, to be honest."));
        }
        else
        {
            _history.Add(new TextRecord(HistoryRecordSource.Customer, "Of course! It’s been in the family for years."));
        }
    }

    public bool MakeDiscountOffer(float discount)
    {
        var newOffer = Mathf.RoundToInt(_agreedOffer * (1f - discount));
        _history.Add(new TextRecord(HistoryRecordSource.Player, $"How about {newOffer}?"));
        Debug.Log($"Trying discount: {discount * 100}% => New offer: {newOffer}");
        var accepted = TryCounterOffer(newOffer);

        if (accepted)
        {
            Debug.Log($"Counter offer accepted: {newOffer}");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, $"Okay, let's do {newOffer}."));
        }
        else
        {
            Debug.Log("Counter offer rejected.");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, $"No way. That's too low."));
        }

        return accepted;
    }
}