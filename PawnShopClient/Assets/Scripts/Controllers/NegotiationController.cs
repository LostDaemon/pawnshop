using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Services;
using System;

namespace PawnShop.Controllers
{
    public class NegotiationController : MonoBehaviour
    {
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _askDiscountButton;
        [SerializeField] private Button _askButton;
        [SerializeField] private Button _analyzeButton;
        [SerializeField] private ItemDetailsController _itemDetailsController;
        [SerializeField] private CounterOfferDialogController _counterOfferDialogController;

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
            _negotiationService.OnNegotiationStarted += OnNegotiationStarted;
            _negotiationService.OnCurrentOfferChanged += OnCurrentOfferChanged;
            _negotiationService.OnTagsRevealed += OnTagsRevealed;

            // Set up counter offer dialog events
            if (_counterOfferDialogController != null)
            {
                _counterOfferDialogController.OnTagsConfirmed += OnTagsConfirmed;
                _counterOfferDialogController.OnDialogCancelled += OnDialogCancelled;
            }
        }

        private void OnNegotiationStarted()
        {
            var item = _negotiationService.CurrentItem;
            _discountButton.Button.interactable = true;
            _itemDetailsController.UpdateItemDetails(item);
        }

        /// <summary>
        /// Show the counter offer dialog
        /// </summary>
        public void ShowCounterOfferDialog()
        {
            if (_counterOfferDialogController != null)
            {
                _counterOfferDialogController.Show();
            }
            else
            {
                Debug.LogWarning("CounterOfferDialogController is not assigned");
            }
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
                _negotiationService.OnNegotiationStarted -= OnNegotiationStarted;
                _negotiationService.OnCurrentOfferChanged -= OnCurrentOfferChanged;
            }

            if (_counterOfferDialogController != null)
            {
                _counterOfferDialogController.OnTagsConfirmed -= OnTagsConfirmed;
                _counterOfferDialogController.OnDialogCancelled -= OnDialogCancelled;
            }
        }

        private void OnBuyClicked()
        {
            long offer = _negotiationService.GetCurrentOffer();

            if (_negotiationService.TryMakeDeal(offer))
            {
                Debug.Log("Deal confirmed.");
            }
            else
            {
                Debug.Log("Deal failed.");
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
            ShowCounterOfferDialog();
        }

        private void OnTagsConfirmed(List<BaseTagModel> selectedTags)
        {
            Debug.Log($"Counter offer dialog: {selectedTags.Count} tags confirmed");
            // TODO: Implement counter offer logic based on selected tags
            _counterOfferDialogController?.Hide();
        }

        private void OnDialogCancelled()
        {
            Debug.Log("Counter offer dialog cancelled");
            _counterOfferDialogController?.Hide();
        }
    }
}