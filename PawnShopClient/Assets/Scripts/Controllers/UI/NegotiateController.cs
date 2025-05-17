using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class NegotiationController : MonoBehaviour
{
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _skipButton;

    [SerializeField] private Button _offer10Button;
    [SerializeField] private Button _offer25Button;
    [SerializeField] private Button _offer50Button;
    [SerializeField] private Button _offer75Button;

    [SerializeField] private IndicatorController _currentOfferIndicator;

    [SerializeField] private TMP_Text _itemNameLabel;
    [SerializeField] private TMP_Text _statusText;

    private INegotiateService _purchaseService;
    private long _lastPlayerOffer;
    private long? _agreedPrice;

    [Inject]
    public void Construct(INegotiateService purchaseService)
    {
        _purchaseService = purchaseService;

        _buyButton.onClick.AddListener(OnBuyClicked);
        _skipButton.onClick.AddListener(OnSkipClicked);

        _offer10Button.onClick.AddListener(() => MakeDiscountOffer(0.10f));
        _offer25Button.onClick.AddListener(() => MakeDiscountOffer(0.25f));
        _offer50Button.onClick.AddListener(() => MakeDiscountOffer(0.50f));
        _offer75Button.onClick.AddListener(() => MakeDiscountOffer(0.75f));

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
        _lastPlayerOffer = _purchaseService.CurrentNpcOffer;
        _agreedPrice = null;

        _itemNameLabel.text = item.Name;
        _statusText.text = "";
        _currentOfferIndicator.SetValue(_lastPlayerOffer, animate: true);
    }

    private void OnBuyClicked()
    {
        if (_purchaseService.TryPurchase(_lastPlayerOffer))
        {
            Debug.Log("Purchase confirmed.");
        }
        else
        {
            Debug.Log("Purchase failed.");
        }
    }

    private void OnSkipClicked()
    {
        _purchaseService.RequestSkip();
    }

    private void MakeDiscountOffer(float discount)
    {
        var offer = Mathf.RoundToInt(_lastPlayerOffer * (1f - discount));

        if (_agreedPrice == offer)
        {
            _currentOfferIndicator.SetValue(offer, animate: true);
            _statusText.text = $"NPC already accepted: {offer}";
            return;
        }

        if (_purchaseService.TryCounterOffer(offer))
        {
            _lastPlayerOffer = offer;
            _agreedPrice = offer;
            _currentOfferIndicator.SetValue(offer, animate: true);
            _statusText.text = $"NPC accepted offer: {offer}";
            Debug.Log($"Counter offer accepted: {offer}");
        }
        else
        {
            _statusText.text = $"NPC rejected offer: {offer}";
            Debug.Log("Counter offer rejected.");
        }
    }
}