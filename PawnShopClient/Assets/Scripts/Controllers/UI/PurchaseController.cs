using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PurchaseController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _priceInput;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _skipButton;

    private IPurchaseService _purchaseService;
    private IItemRepositoryService _itemRepository;

    [Inject]
    public void Construct(IPurchaseService purchaseService, IItemRepositoryService itemRepository)
    {
        _itemRepository = itemRepository;
        _purchaseService = purchaseService;
        _buyButton.onClick.AddListener(OnBuyClicked);
        _skipButton.onClick.AddListener(OnSkipClicked);
    }

    private void OnBuyClicked()
    {
        if (!long.TryParse(_priceInput.text, out var offeredPrice))
        {
            Debug.LogWarning("Invalid price input.");
            return;
        }

        var success = _purchaseService.TryPurchase(offeredPrice);
        if (!success)
        {
            Debug.Log("Purchase failed: not enough money or no item.");
        }
    }

    private void OnSkipClicked()
    {
        _purchaseService.RequestSkip();
    }
}