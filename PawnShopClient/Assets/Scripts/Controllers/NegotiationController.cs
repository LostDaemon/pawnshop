using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using PawnShop.Controllers;
using System.Collections.Generic;
using System;

public class NegotiationController : MonoBehaviour
{
    [SerializeField] private Button _buyButton;
    [SerializeField] private Button _skipButton;
    [SerializeField] private Button _askDiscountButton;
    [SerializeField] private Button _askButton;
    [SerializeField] private Button _analyzeButton;
    [SerializeField] private ItemDetailsController _itemDetailsController;

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
        _negotiationService.OnCurrentOfferChanged += OnCurrentOfferChanged;
        _negotiationService.OnTagsRevealed += OnTagsRevealed;
    }

    private void OnTagsRevealed(ItemModel item)
    {
        _itemDetailsController?.UpdateItemDetails(item);
    }

    private void OnCurrentOfferChanged(ItemModel item)
    {
        _itemDetailsController?.UpdateItemDetails(item);
    }

    private void OnAnalyzeClicked()
    {
        _negotiationService.AnalyzeItem();
    }

    private void OnDestroy()
    {
        if (_negotiationService != null)
        {
            _negotiationService.OnCurrentItemChanged -= OnItemChanged;
            _negotiationService.OnCurrentOfferChanged -= OnCurrentOfferChanged;
        }
    }

    private void OnItemChanged(ItemModel item)
    {
        if (item == null)
        {
            Debug.LogError("NegotiationController: Received null item in OnItemChanged.");
            return;
        }

        _discountButton.Button.interactable = true;
        _itemDetailsController.UpdateItemDetails(item);
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
            Debug.Log($"Discount offer accepted: {_negotiationService.GetCurrentOffer()}");
        }
        else
        {
            _discountButton.Button.interactable = false;
        }
    }
}