using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System;

public class NegotiationController : MonoBehaviour
{
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _skipButton;

    [SerializeField] private Button _askDiscountButton;
    [SerializeField] private Button _askButton;

    [SerializeField] private Button _analyzeButton;

    [SerializeField] private IndicatorController _currentOfferIndicator;
    [SerializeField] private TMP_Text _itemNameLabel;

    private INegotiationService _negotiationService;

    private struct DiscountButton
    {
        public float Discount;
        public Button Button;
    }

    private DiscountButton _discountButton;

    [Inject]
    public void Construct(INegotiationService negotiationService)
    {
        _negotiationService = negotiationService;

        _buyButton?.onClick.AddListener(OnBuyClicked);
        _skipButton?.onClick.AddListener(OnSkipClicked);
        _askButton?.onClick.AddListener(OnAskClicked);
        _analyzeButton?.onClick.AddListener(OnAnalyzeClicked);

        _discountButton = new DiscountButton { Discount = 0.10f, Button = _askDiscountButton };
        _discountButton.Button.onClick.AddListener(() => OnDiscountClicked(_discountButton.Discount));
        _negotiationService.OnCurrentItemChanged += OnItemChanged;
    }

    private void OnAnalyzeClicked()
    {
        _negotiationService.AnalyzeItem();
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

        if (_itemNameLabel != null) _itemNameLabel.text = item.Name;
        _currentOfferIndicator.SetValue(_negotiationService.GetCurrentOffer(), animate: true);

        _discountButton.Button.interactable = true;
    }

    private void OnBuyClicked()
    {
        long offer = _negotiationService.GetCurrentOffer();

        if (_negotiationService.TryPurchase(offer))
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
        _negotiationService.RequestSkip();
    }

    private void OnAskClicked()
    {
        _negotiationService.AskAboutItemOrigin();
    }

    private void OnDiscountClicked(float discount)
    {
        var response = _negotiationService.MakeDiscountOffer(discount);
        if (response)
        {
            _currentOfferIndicator.SetValue(_negotiationService.GetCurrentOffer(), animate: true);
            Debug.Log($"Discount offer accepted: {_negotiationService.GetCurrentOffer()}");
        }
        else
        {
            _discountButton.Button.interactable = false;
        }
    }
}