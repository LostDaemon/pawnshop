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
    private readonly System.Random _random = new();

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;
    public event Action<long> OnCurrentOfferChanged;
    public event Action OnSkipRequested;
    public event Action OnTagsRevealed;

    public ItemModel CurrentItem { get; private set; }

    public long CurrentNpcOffer { get; private set; }
    private long _agreedOffer;
    private readonly HashSet<long> _rejectedOffers = new();

    [Inject]
    public NegotiationService(
        IWalletService wallet,
        [Inject(Id = StorageType.InventoryStorage)] IGameStorageService<ItemModel> inventory,
        ICustomerService customerService,
        INegotiationHistoryService history,
        ILocalizationService localizationService)
    {
        _wallet = wallet;
        _inventory = inventory;
        _customerService = customerService;
        _history = history;
        _localizationService = localizationService;
    }

    public void SetCurrentCustomer(Customer customer)
    {
        _customerService.SetCurrent(customer);
        CurrentItem = customer.OwnedItem;

        GenerateInitialNpcOffer();
        _agreedOffer = CurrentNpcOffer;
        _rejectedOffers.Clear();

        _history.Add(new TextRecord(HistoryRecordSource.Customer, 
            string.Format(_localizationService.GetLocalization("dialog_customer_initial_offer"), CurrentItem.Name, CurrentNpcOffer)));
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
        _history.Add(new TextRecord(HistoryRecordSource.Player, 
            string.Format(_localizationService.GetLocalization("dialog_player_skip_item"), CurrentItem?.Name)));
        OnSkipRequested?.Invoke();
    }

    public void AskAboutItemOrigin()
    {
        if (CurrentItem == null || _customerService.Current == null)
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
        var newOffer = Mathf.RoundToInt(_agreedOffer * (1f - discount));
        _history.Add(new TextRecord(HistoryRecordSource.Player, 
            string.Format(_localizationService.GetLocalization("dialog_player_counter_offer"), newOffer)));
        Debug.Log($"Trying discount: {discount * 100}% => New offer: {newOffer}");
        var accepted = TryCounterOffer(newOffer);

        if (accepted)
        {
            _agreedOffer = newOffer;
            Debug.Log($"Counter offer accepted: {newOffer}");
            _history.Add(new TextRecord(HistoryRecordSource.Customer, 
                string.Format(_localizationService.GetLocalization("dialog_customer_counter_accepted"), newOffer)));
            OnCurrentOfferChanged?.Invoke(_agreedOffer);
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
        // Temporary implementation - reveals all tags
        // TODO: Replace with complex analysis logic in the future
        
        if (CurrentItem?.Tags == null)
            return;
            
        int revealedCount = 0;
        foreach (var tag in CurrentItem.Tags)
        {
            if (tag != null && !tag.IsRevealed)
            {
                tag.IsRevealed = true;
                revealedCount++;
                Debug.Log($"Tag revealed through analysis: {tag.TagType} - {tag.DisplayName}");
            }
        }
        
        if (revealedCount > 0)
        {
            Debug.Log($"Analysis complete: {revealedCount} tags revealed");
            
            // Add to history
            _history.Add(new TextRecord(HistoryRecordSource.Player, 
                string.Format(_localizationService.GetLocalization("dialog_player_analyzed_item"), CurrentItem.Name, revealedCount)));
            
            // Trigger UI update event
            OnCurrentItemChanged?.Invoke(CurrentItem);
            
            // Notify that tags were revealed
            OnTagsRevealed?.Invoke();
        }
        else
        {
            Debug.Log("Analysis complete: no new tags to reveal");
            _history.Add(new TextRecord(HistoryRecordSource.Player, 
                _localizationService.GetLocalization("dialog_player_analysis_no_new_info")));
        }
    }
    
    public List<BaseTagModel> GetVisibleTags()
    {
        if (CurrentItem?.Tags == null)
            return new List<BaseTagModel>();
            
        return CurrentItem.Tags.Where(tag => tag != null && IsTagVisible(tag)).ToList();
    }
    
    public bool IsTagVisible(BaseTagModel tag)
    {
        if (tag == null)
            return false;
            
        // Tag is visible if it's marked as revealed by default
        // or if it was revealed through gameplay mechanics
        return tag.IsRevealed;
    }
    
    public void RevealTag(BaseTagModel tag)
    {
        if (tag == null)
            return;
            
        tag.IsRevealed = true;
        Debug.Log($"Tag revealed: {tag.TagType} - {tag.DisplayName}");
        
        // You can add additional logic here, such as:
        // - Adding to history
        // - Triggering events
        // - Updating UI
    }
}