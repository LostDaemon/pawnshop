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
    [SerializeField] private Button _askButton;

    [SerializeField] private IndicatorController _currentOfferIndicator;
    [SerializeField] private TMP_Text _itemNameLabel;
    [SerializeField] private SpeechPopupController _speechPopup;

    private INegotiationService _negotiationService;

    private struct DiscountButton
    {
        public float Discount;
        public Button Button;
    }

    private DiscountButton[] _discountButtons;

    [Inject]
    public void Construct(INegotiationService negotiationService)
    {
        _negotiationService = negotiationService;

        _buyButton.onClick.AddListener(OnBuyClicked);
        _skipButton.onClick.AddListener(OnSkipClicked);
        _askButton.onClick.AddListener(OnAskClicked);

        _discountButtons = new[]
        {
            new DiscountButton { Discount = 0.10f, Button = _offer10Button },
            new DiscountButton { Discount = 0.25f, Button = _offer25Button },
            new DiscountButton { Discount = 0.50f, Button = _offer50Button },
            new DiscountButton { Discount = 0.75f, Button = _offer75Button },
        };

        foreach (var db in _discountButtons)
        {
            float d = db.Discount;
            db.Button.onClick.AddListener(() => MakeDiscountOffer(d));
        }

        _negotiationService.OnCurrentItemChanged += OnItemChanged;
    }

    private void OnDestroy()
    {
        if (_negotiationService != null)
            _negotiationService.OnCurrentItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(ItemModel item)
    {
        if (item == null)
        {
            Debug.LogError("NegotiationController: Received null item in OnItemChanged.");
            return;
        }

        _itemNameLabel.text = item.Name;
        _currentOfferIndicator.SetValue(_negotiationService.GetCurrentOffer(), animate: true);

        foreach (var db in _discountButtons)
            db.Button.interactable = true;

        _speechPopup.ShowMessage($"How about {_negotiationService.CurrentNpcOffer}?");
    }

    private void OnBuyClicked()
    {
        long offer = _negotiationService.GetCurrentOffer();

        if (_negotiationService.TryPurchase(offer))
        {
            _speechPopup.ShowMessage($"Deal. {offer} it is.");
            Debug.Log("Purchase confirmed.");
        }
        else
        {
            _speechPopup.ShowMessage("You sure you can pay that?");
            Debug.Log("Purchase failed.");
        }
    }

    private void OnSkipClicked()
    {
        _negotiationService.RequestSkip();
    }

    private void OnAskClicked()
    {
        _negotiationService.AskAboutItemOrigin();
    }

    private void MakeDiscountOffer(float discount)
    {
        if (_negotiationService.TryDiscount(discount, out var newOffer, out var accepted, out var toBlock))
        {
            if (accepted)
            {
                _currentOfferIndicator.SetValue(newOffer, animate: true);
                _speechPopup.ShowMessage($"Alright, we can do {newOffer}.");
                Debug.Log($"Counter offer accepted: {newOffer}");

                foreach (var db in _discountButtons)
                    db.Button.interactable = true;
            }
            else
            {
                _speechPopup.ShowMessage("No way. That's too low.");
                Debug.Log("Counter offer rejected.");

                foreach (var db in _discountButtons)
                {
                    if (toBlock.Contains(db.Discount))
                        db.Button.interactable = false;
                }
            }
        }
    }
}