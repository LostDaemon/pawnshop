using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class NegotiationService : INegotiationService
{
    private readonly IWalletService _wallet;
    private readonly IGameStorageService<ItemModel> _inventory;
    private readonly ICustomerService _customerService;
    private readonly INegotiationHistoryService _history;
    private readonly ILocalizationService _localizationService;
    private readonly IItemInspectionService _inspectionService;
    private readonly IPlayerService _playerService;
    private readonly System.Random _random = new();

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;
    public event Action<ItemModel> OnCurrentOfferChanged;
    public event Action OnSkipRequested;
    public event Action<ItemModel> OnTagsRevealed;

    public ItemModel CurrentItem => _customerService.CurrentCustomer?.OwnedItem;
    public Customer CurrentCustomer => _customerService.CurrentCustomer;
    private readonly HashSet<long> _rejectedOffers = new();

    [Inject]
    public NegotiationService(
        IWalletService wallet,
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        ICustomerService customerService,
        INegotiationHistoryService history,
        ILocalizationService localizationService,
        IItemInspectionService inspectionService,
        IPlayerService playerService)
    {
        _wallet = wallet;
        _inventory = inventory;
        _customerService = customerService;
        _history = history;
        _localizationService = localizationService;
        _inspectionService = inspectionService;
        _playerService = playerService;
    }

    public long GetCurrentOffer() => CurrentItem?.CurrentOffer ?? 0;

    public void ShowNextCustomer()
    {
        _customerService.ShowNextCustomer();
        GenerateInitialNpcOffer();
        _rejectedOffers.Clear();
        _history.Add(new TextRecord(HistoryRecordSource.Customer,
            string.Format(_localizationService.GetLocalization("dialog_customer_initial_offer"), CurrentItem.Name, CurrentItem.CurrentOffer)));
        OnCurrentItemChanged?.Invoke(CurrentItem);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;
        CurrentItem.CurrentOffer = _customerService.EvaluateCurrentItem();
    }

    public bool TryPurchase(long offeredPrice)
    {
        if (CurrentItem == null)
            return false;

        var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
        if (!success)
        {
            _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_cant_pay")));
            return false;
        }

        CurrentItem.PurchasePrice = offeredPrice;
        _inventory.Put(CurrentItem);
        _history.Add(new TextRecord(HistoryRecordSource.Customer,
            string.Format(_localizationService.GetLocalization("dialog_customer_deal_accepted"), offeredPrice)));
        OnPurchased?.Invoke(CurrentItem);
        return true;
    }

    public bool TryCounterOffer(long playerOffer)
    {
        if (CurrentItem == null)
            return false;

        if (_rejectedOffers.Contains(playerOffer))
            return false;

        var minAcceptable = (long)(CurrentItem.BasePrice * 0.6f * (1f - CurrentCustomer.UncertaintyLevel));
        var maxAcceptable = (long)(CurrentItem.BasePrice * 0.95f);

        Debug.Log($"NPC offer: {CurrentItem.CurrentOffer}, Player offer: {playerOffer}, Min: {minAcceptable}, Max: {maxAcceptable}");

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
        _history.Add(new TextRecord(HistoryRecordSource.Player,
            string.Format(_localizationService.GetLocalization("dialog_player_skip_item"), CurrentItem?.Name)));
        OnSkipRequested?.Invoke();
    }

    public void AskAboutItemOrigin()
    {
        if (CurrentItem == null || CurrentCustomer == null)
            return;

        if (CurrentItem.IsFake)
        {
            _customerService.IncreaseUncertainty(0.25f);
            _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_uncertain_origin")));
        }
        else
        {
            _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_family_item")));
        }
    }

    public bool MakeDiscountOffer(float discount)
    {
        var newOffer = Mathf.RoundToInt(CurrentItem.CurrentOffer * (1f - discount));
        _history.Add(new TextRecord(HistoryRecordSource.Player,
            string.Format(_localizationService.GetLocalization("dialog_player_counter_offer"), newOffer)));
        Debug.Log($"Trying discount: {discount * 100}% => New offer: {newOffer}");
        var accepted = TryCounterOffer(newOffer);

        if (accepted)
        {
            CurrentItem.CurrentOffer = newOffer;
            Debug.Log($"Counter offer accepted: {newOffer}");
            _history.Add(new TextRecord(HistoryRecordSource.Customer,
                string.Format(_localizationService.GetLocalization("dialog_customer_counter_accepted"), newOffer)));
            OnCurrentOfferChanged?.Invoke(CurrentItem);
        }
        else
        {
            Debug.Log("Counter offer rejected.");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, _localizationService.GetLocalization("dialog_customer_counter_rejected")));
        }

        return accepted;
    }

    public void AnalyzeItem()
    {
        var revealedTags = _inspectionService.Inspect(_playerService.Player, CurrentItem);

        // Add to history
        _history.Add(new TextRecord(HistoryRecordSource.Player,
            string.Format(_localizationService.GetLocalization("dialog_player_analyzed_item"), CurrentItem.Name, revealedTags.Count)));

        // Notify that tags were revealed
        OnTagsRevealed?.Invoke(CurrentItem);
    }
}