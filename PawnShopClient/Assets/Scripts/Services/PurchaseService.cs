using System;

public class PurchaseService : IPurchaseService
{
    private readonly IWalletService _wallet;
    private readonly IGameStorageService<ItemModel> _inventory;

    public event Action<ItemModel> OnPurchased;
    public event Action<ItemModel> OnCurrentItemChanged;

    public ItemModel CurrentItem { get; private set; }

    public PurchaseService(IWalletService wallet, IGameStorageService<ItemModel> inventory)
    {
        _wallet = wallet;
        _inventory = inventory;
    }

    public void SetCurrentItem(ItemModel item)
    {
        CurrentItem = item;
        OnCurrentItemChanged?.Invoke(item);
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
}