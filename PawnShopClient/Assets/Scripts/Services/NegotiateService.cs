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
    public event Action<float> OnCustomerUncertaintyChanged;
    public event Action<float> OnCustomerMoodChanged;

    public ItemModel CurrentItem { get; private set; }
    public Customer CurrentCustomer { get; private set; }
    public long CurrentNpcOffer { get; private set; }

    private long _agreedOffer;
    private readonly HashSet<long> _rejectedOffers = new();

    private static readonly float[] AllDiscounts = { 0.10f, 0.25f, 0.50f, 0.75f };


    private readonly List<IHistoryRecord> _history = new();
    public IReadOnlyList<IHistoryRecord> History => _history.AsReadOnly();
    public event Action<IHistoryRecord> OnRecordAdded;

    private void AddHistory(IHistoryRecord record)
    {
        _history.Add(record);
        OnRecordAdded?.Invoke(record);
    }

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

        AddHistory(new TextRecord("Customer", $"I'd sell '{CurrentItem.Name}' for {CurrentNpcOffer}."));
        OnCurrentItemChanged?.Invoke(CurrentItem);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;
        CurrentNpcOffer = (long)(CurrentItem.RealPrice * _random.Next(60, 86) / 100f);
    }

    public long GetCurrentOffer() => _agreedOffer;

    public bool TryDiscount(float discount, out long newOffer, out bool accepted, out List<float> discountsToBlock) //<-- out 3x ???!!!
    {
        newOffer = Mathf.RoundToInt(_agreedOffer * (1f - discount));
        Debug.Log($"Trying discount: {discount * 100}% => New offer: {newOffer}");

        accepted = TryCounterOffer(newOffer);
        discountsToBlock = new List<float>();

        AddHistory(new TextRecord("Player", $"Offered {newOffer} ({discount * 100}% discount)"));

        if (accepted)
        {
            _agreedOffer = newOffer;
            AddHistory(new TextRecord("Customer", $"Okay, let's do {newOffer}."));
        }
        else
        {
            AddHistory(new TextRecord("Customer", "No way. Too low."));
            ChangeCustomerMood(-0.25f);
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
            AddHistory(new TextRecord("System", $"Not enough money to buy '{CurrentItem.Name}' for {offeredPrice}."));
            return false;
        }

        _inventory.Put(CurrentItem);
        AddHistory(new TextRecord("System", $"Item '{CurrentItem.Name}' purchased for {offeredPrice}."));
        OnPurchased?.Invoke(CurrentItem);
        return true;
    }

    public bool TryCounterOffer(long playerOffer)
    {
        if (CurrentItem == null)
            return false;

        if (_rejectedOffers.Contains(playerOffer))
            return false;

        var minAcceptable = (long)(CurrentItem.RealPrice * 0.6f * (1f - CurrentCustomer.UncertaintyLevel));
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
        AddHistory(new TextRecord("Player", $"Skipped '{CurrentItem?.Name}'"));
        OnSkipRequested?.Invoke();
    }

    public void AskAboutItemOrigin()
    {
        if (CurrentItem == null || CurrentCustomer == null)
            return;

        if (CurrentItem.IsFake)
        {
            IncreaseCustomerUncertainty(0.25f);
            AddHistory(new TextRecord("Customer", "Umm... I'm not sure where this item came from, to be honest."));
        }
        else
        {
            AddHistory(new TextRecord("Customer", "Of course! It’s been in the family for years."));
        }
    }

    private void IncreaseCustomerUncertainty(float amount)
    {
        if (CurrentCustomer == null)
            return;

        CurrentCustomer.UncertaintyLevel = Mathf.Clamp01(CurrentCustomer.UncertaintyLevel + amount);
        OnCustomerUncertaintyChanged?.Invoke(CurrentCustomer.UncertaintyLevel);
        ChangeCustomerMood(-0.1f);
        Debug.Log($"Customer's uncertainty level increased to {CurrentCustomer.UncertaintyLevel}");
    }

    private void ChangeCustomerMood(float amount)
    {
        if (CurrentCustomer == null)
            return;

        CurrentCustomer.MoodLevel = Mathf.Clamp(CurrentCustomer.MoodLevel + amount, -1, 1);
        OnCustomerMoodChanged?.Invoke(CurrentCustomer.MoodLevel);

        Debug.Log($"Customer's mood level is changed to {CurrentCustomer.MoodLevel}");
    }
}