using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PurchaseController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _priceInput;
    [SerializeField] private Button _buyButton;

    private IWalletService _wallet;
    private IGameStorageService<ItemModel> _inventory;
    private ItemModel _currentItem;

    [Inject]
    public void Construct(IWalletService wallet, IGameStorageService<ItemModel> inventory)
    {
        _wallet = wallet;
        _inventory = inventory;

        _buyButton.onClick.AddListener(OnBuyClicked);
    }

    public void SetItem(ItemModel item)
    {
        _currentItem = item;
    }

    private void OnBuyClicked()
    {
        if (_currentItem == null)
            return;

        if (!int.TryParse(_priceInput.text, out var offeredPrice))
            return;

        var success = _wallet.TransactionAttempt(CurrencyType.Money, -offeredPrice);
        if (!success)
        {
            Debug.Log("Not enough money!");
            return;
        }

        _inventory.Put(_currentItem);
        Debug.Log($"Item purchased: {_currentItem.Name} for {offeredPrice}");
    }
}