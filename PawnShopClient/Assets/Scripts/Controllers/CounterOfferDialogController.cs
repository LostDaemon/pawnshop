using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using PawnShop.Models;
using PawnShop.Services;
using PawnShop.Models.Tags;

namespace PawnShop.Controllers
{
    public class CounterOfferDialogController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _tagListItemPrefab;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TMP_InputField _offerInputField;

        private INegotiationService _negotiationService;
        private List<TagSelectorListItemController> _tagListItems = new List<TagSelectorListItemController>();
        private ItemModel _currentItem;

        public event Action<List<BaseTagModel>> OnTagsConfirmed;
        public event Action OnDialogCancelled;

        /// <summary>
        /// Get the current offer value from input field
        /// </summary>
        public string CurrentOfferText => _offerInputField?.text ?? string.Empty;

        [Inject]
        public void Construct(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService;

            // Subscribe to negotiation service events
            _negotiationService.OnCurrentItemChanged += OnCurrentItemChanged;
            _negotiationService.OnTagsRevealed += OnTagsRevealed;

            // Set up button listeners
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDestroy()
        {
            if (_negotiationService != null)
            {
                _negotiationService.OnCurrentItemChanged -= OnCurrentItemChanged;
                _negotiationService.OnTagsRevealed -= OnTagsRevealed;
            }

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(OnCancelClicked);

            ClearTagListItems();
        }

        private void OnCurrentItemChanged(ItemModel item)
        {
            _currentItem = item;
            RefreshTagsDisplay();
        }

        private void OnTagsRevealed(ItemModel item)
        {
            _currentItem = item;
            RefreshTagsDisplay();
        }

        /// <summary>
        /// Refresh the tags display for the current item
        /// </summary>
        public void RefreshTagsDisplay()
        {
            ClearTagListItems();

            if (_currentItem == null)
            {
                Debug.LogWarning("CounterOfferDialogController: No current item to display tags for.");
                return;
            }

            CreateTagListItems();
        }

        private void CreateTagListItems()
        {
            if (_tagListItemPrefab == null)
            {
                Debug.LogError("CounterOfferDialogController: Tag list item prefab is not assigned.");
                return;
            }

            if (_contentRoot == null)
            {
                Debug.LogError("CounterOfferDialogController: Content root is not assigned.");
                return;
            }

            var revealedTags = _currentItem.Tags.Where(tag => tag.IsRevealedToPlayer).ToList();

            if (revealedTags.Count == 0)
            {
                return;
            }

            foreach (var tag in revealedTags)
            {
                var listItemObject = Instantiate(_tagListItemPrefab, _contentRoot);
                var listItemController = listItemObject.GetComponent<TagSelectorListItemController>();

                if (listItemController == null)
                {
                    Destroy(listItemObject);
                    continue;
                }

                // Initialize the list item with tag data
                listItemController.Init(tag);

                // Store reference
                _tagListItems.Add(listItemController);
            }
        }

        private void ClearTagListItems()
        {
            foreach (var listItem in _tagListItems)
            {
                if (listItem != null)
                    Destroy(listItem.gameObject);
            }

            _tagListItems.Clear();
        }

        private void OnConfirmClicked()
        {

            if (string.IsNullOrEmpty(CurrentOfferText))
            {
                return;
            }

            if (long.TryParse(CurrentOfferText, out var newOffer) == false)
            {
                return;
            }

            var selectedTags = GetSelectedTags();

            // Declare selected tags to the customer
            _negotiationService.DeclareTags(selectedTags);
            _negotiationService.MakeCounterOffer(newOffer);
            OnTagsConfirmed?.Invoke(selectedTags);
        }

        private void OnCancelClicked()
        {
            OnDialogCancelled?.Invoke();
            Debug.Log("CounterOfferDialogController: Dialog cancelled.");
        }

        /// <summary>
        /// Get the list of currently selected tags
        /// </summary>
        /// <returns>List of selected tag models</returns>
        public List<BaseTagModel> GetSelectedTags()
        {
            var selectedTags = new List<BaseTagModel>();

            if (_currentItem == null || _tagListItems.Count == 0)
                return selectedTags;

            foreach (var listItem in _tagListItems)
            {
                if (listItem != null && listItem.GetToggleState())
                {
                    selectedTags.Add(listItem.Tag);
                }
            }

            return selectedTags;
        }

        /// <summary>
        /// Set the current item and refresh the display
        /// </summary>
        /// <param name="item">Item to display tags for</param>
        public void SetItem(ItemModel item)
        {
            _currentItem = item;
            RefreshTagsDisplay();
        }

        /// <summary>
        /// Show the dialog
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshTagsDisplay();
        }

        /// <summary>
        /// Hide the dialog
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the offer value in the input field
        /// </summary>
        /// <param name="offerValue">Offer value to display</param>
        public void SetOfferValue(string offerValue)
        {
            if (_offerInputField != null)
            {
                _offerInputField.text = offerValue;
            }
        }

        /// <summary>
        /// Clear the offer input field
        /// </summary>
        public void ClearOfferInput()
        {
            if (_offerInputField != null)
            {
                _offerInputField.text = string.Empty;
            }
        }
    }
}
