using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PurchaseController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _priceInput;
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _skipButton;
    [SerializeField] private TMP_Text _npcOfferText;

    private INegotiateService _purchaseService;
    private IItemRepositoryService _itemRepository;

    [Inject]
    public void Construct(INegotiateService purchaseService, IItemRepositoryService itemRepository)
    {
        _itemRepository = itemRepository;
        _purchaseService = purchaseService;

        _buyButton.onClick.AddListener(OnBuyClicked);
        _skipButton.onClick.AddListener(OnSkipClicked);

        _purchaseService.OnCurrentItemChanged += OnItemChanged;

        if (_purchaseService.CurrentItem != null)
            OnItemChanged(_purchaseService.CurrentItem);
    }

    private void OnDestroy()
    {
        if (_purchaseService != null)
            _purchaseService.OnCurrentItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(ItemModel item)
    {
        _npcOfferText.text = $"Customer offers: {_purchaseService.CurrentNpcOffer}";
        _priceInput.text = "";
    }

    private void OnBuyClicked()
    {
        if (!long.TryParse(_priceInput.text, out var offeredPrice))
        {
            Debug.LogWarning("Invalid price input.");
            return;
        }

        // Если цена равна NPC-предложению — просто покупка
        if (offeredPrice == _purchaseService.CurrentNpcOffer)
        {
            if (_purchaseService.TryPurchase(offeredPrice))
                Debug.Log("Offer accepted.");
            else
                Debug.Log("Purchase failed.");
        }
        else
        {
            // Торг: проверяем, согласен ли NPC
            if (_purchaseService.TryCounterOffer(offeredPrice))
            {
                _purchaseService.TryPurchase(offeredPrice);
                Debug.Log($"Counter offer accepted: {offeredPrice}");
            }
            else
            {
                Debug.Log("Counter offer rejected.");
            }
        }
    }

    private void OnSkipClicked()
    {
        _purchaseService.RequestSkip();
    }
}