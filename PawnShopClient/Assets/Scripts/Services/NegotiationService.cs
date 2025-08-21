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
    private readonly IEvaluationService _evaluationService;

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;
    public event Action<ItemModel> OnCurrentOfferChanged;
    public event Action OnSkipRequested;
    public event Action<ItemModel> OnTagsRevealed;

    public ItemModel CurrentItem => _customerService.CurrentCustomer?.OwnedItem;
    public Customer CurrentCustomer => _customerService.CurrentCustomer;


    [Inject]
    public NegotiationService(
        IWalletService wallet,
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        ICustomerService customerService,
        INegotiationHistoryService history,
        ILocalizationService localizationService,
        IItemInspectionService inspectionService,
        IPlayerService playerService,
        IEvaluationService evaluationService)
    {
        _wallet = wallet;
        _inventory = inventory;
        _customerService = customerService;
        _history = history;
        _localizationService = localizationService;
        _inspectionService = inspectionService;
        _playerService = playerService;
        _evaluationService = evaluationService;
    }

    public long GetCurrentOffer() => CurrentItem?.CurrentOffer ?? 0;

    public void ShowNextCustomer()
    {
        _customerService.ShowNextCustomer();
        GenerateInitialNpcOffer();

        _history.Add(new TextRecord(HistoryRecordSource.Customer,
            string.Format(_localizationService.GetLocalization("dialog_customer_initial_offer"), CurrentItem.Name, CurrentItem.CurrentOffer)));
        OnCurrentItemChanged?.Invoke(CurrentItem);
    }

    private void GenerateInitialNpcOffer()
    {
        if (CurrentItem == null) return;
        // Use evaluation service to get customer's initial offer based on revealed tags
        _inspectionService.InspectByCustomer(CurrentItem);
        CurrentItem.CurrentOffer = _evaluationService.EvaluateByCustomer(CurrentItem);
        Debug.Log($"Generated initial NPC offer: {CurrentItem.CurrentOffer} for item: {CurrentItem.Name}");
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

        CurrentItem.CurrentOffer = offeredPrice;

        _history.Add(new TextRecord(HistoryRecordSource.Customer,
            string.Format(_localizationService.GetLocalization("dialog_customer_deal_accepted"), offeredPrice)));
        OnPurchased?.Invoke(CurrentItem);
        return true;
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

    public bool MakeCounterOffer(long newOffer)
    {
        _history.Add(new TextRecord(HistoryRecordSource.Player,
            string.Format(_localizationService.GetLocalization("dialog_player_counter_offer"), newOffer)));

        var accepted = ProcessPlayerOffer(newOffer);

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
        var revealedTags = _inspectionService.InspectByPlayer(CurrentItem);

        // Add to history
        _history.Add(new TextRecord(HistoryRecordSource.Player,
        string.Format(_localizationService.GetLocalization("dialog_player_analyzed_item"), CurrentItem.Name, revealedTags.Count)));
        // Notify that tags were revealed
        OnTagsRevealed?.Invoke(CurrentItem);
    }

    public void DeclareTags(List<BaseTagModel> tags)
    {
        foreach (var tag in tags)
        {
            var curTag = CurrentItem.Tags.FirstOrDefault(t => t.ClassId == tag.ClassId);
            if (curTag != null)
            {
                curTag.IsRevealedToCustomer = true;
            }
        }
    }

    private bool ProcessPlayerOffer(long offer)
    {
        long itemValue = _evaluationService.EvaluateByPlayer(CurrentItem);
        const float deviationRange = 0.1f; // Add random deviation ±10%
        float deviation = UnityEngine.Random.Range(-deviationRange, deviationRange);
        long adjustedValue = (long)(itemValue * (1 + deviation));
        // Make decision based on offer vs adjusted value
        Debug.Log($"Player offer: {offer}, Adjusted value: {adjustedValue}, Calculated item value: {itemValue}");

        return offer >= adjustedValue;
    }
}