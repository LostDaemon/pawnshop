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
        [SerializeField] private Button _askDiscountButton;
        [SerializeField] private Button _askButton;
        [SerializeField] private Button _analyzeButton;
        [SerializeField] private ItemDetailsController _itemDetailsController;
        [SerializeField] private CounterOfferDialogController _counterOfferDialogController;

        // Analysis buttons
        [SerializeField] private Button _visualBtn;
        [SerializeField] private Button _defectoscopeBtn;
        [SerializeField] private Button _historyBtn;
        [SerializeField] private Button _purityBtn;
        [SerializeField] private Button _documentsBtn;
        [SerializeField] private Button _legalStatusBtn;

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
            _askButton?.onClick.AddListener(OnAskClicked);
            _analyzeButton?.onClick.AddListener(OnAnalyzeClicked);

            // Analysis buttons
            _visualBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.VisualAnalysis));
            _defectoscopeBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.Defectoscope));
            _historyBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.HistoryResearch));
            _purityBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.PurityAnalyzer));
            _documentsBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.DocumentInspection));
            _legalStatusBtn?.onClick.AddListener(() => OnAnalysisClicked(AnalyzeType.CheckLegalStatus));

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

        private void OnAnalysisClicked(AnalyzeType analyzeType)
        {
            _negotiationService.AnalyzeItem(analyzeType);
        }

        private void OnDestroy()
        {
            if (_negotiationService != null)
            {
                _negotiationService.OnNegotiationStarted -= OnNegotiationStarted;
                _negotiationService.OnCurrentOfferChanged -= OnCurrentOfferChanged;
                _negotiationService.OnTagsRevealed -= OnTagsRevealed;
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


        private void OnAskClicked()
        {
            var customerTags = _negotiationService.AskAboutItemOrigin();
            // Customer tags are now available for further processing if needed
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