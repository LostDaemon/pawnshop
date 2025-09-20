using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using PawnShop.Models;
using PawnShop.Services;
using PawnShop.Repositories;
using PawnShop.Models.Tags;
using PawnShop.Helpers;

namespace PawnShop.Controllers
{
    public class ItemDetailsController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private IndicatorController itemPriceIndicator;
        [SerializeField] private GameObject purchasePriceIndicator;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemTagsText;
        [SerializeField] private TextMeshProUGUI tagDescription;

        [Header("Settings")]
        [SerializeField] private Color fallbackTagColor = Color.black;

        private ISpriteService _spriteService;
        private ITagService _tagService;

        [Inject]
        private void Construct(ISpriteService spriteService, ITagService tagService)
        {
            _spriteService = spriteService;
            _tagService = tagService;
        }

        /// <summary>
        /// Display item details in the UI
        /// </summary>
        /// <param name="item">Item to display</param>
        public void UpdateItemDetails(ItemModel item)
        {

            if (item == null)
            {
                Debug.LogError("ItemDetailsController: Attempted to show null item details.");
                return;
            }

            UpdateItemName(item);
            UpdateItemDescription(item);
            UpdateItemImage(item);
            UpdateTagsDisplay(item);
            UpdateOfferIndicator(item);
            UpdatePurchasePriceIndicator(item);
            tagDescription.text = string.Empty;
        }

        private void Awake()
        {
            // Enable rich text support for tags text
            if (itemTagsText != null)
            {
                itemTagsText.richText = true;
            }

            // Add click event handler to tags text
            if (itemTagsText != null)
            {
                var eventTrigger = itemTagsText.gameObject.GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = itemTagsText.gameObject.AddComponent<EventTrigger>();
                }

                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => OnTagsTextClicked());
                eventTrigger.triggers.Add(entry);
            }
        }


        private void UpdateItemName(ItemModel item)
        {
            if (item != null && itemNameText != null)
            {
                itemNameText.text = item.Name;
            }
        }

        private void UpdateItemDescription(ItemModel item)
        {
            if (item != null && itemDescriptionText != null)
            {
                itemDescriptionText.text = item.Description;
            }
        }

        private void UpdateItemImage(ItemModel item)
        {
            if (item == null || itemImage == null)
            {
                return;
            }

            var sprite = item.Image;

            if (sprite != null)
            {
                itemImage.sprite = sprite;
                itemImage.preserveAspect = true;
            }
        }

        private void UpdateOfferIndicator(ItemModel item)
        {
            if (item != null && itemPriceIndicator != null)
            {
                itemPriceIndicator.SetValue(item.CurrentOffer, false);
            }
        }

        private void UpdatePurchasePriceIndicator(ItemModel item)
        {
            if (item != null && purchasePriceIndicator != null)
            {
                // Hide purchasePriceIndicator if PurchasePrice is zero
                bool shouldShow = item.PurchasePrice > 0;
                purchasePriceIndicator.SetActive(shouldShow);
                
                if (shouldShow)
                {
                    var indicatorController = purchasePriceIndicator.GetComponentInChildren<IndicatorController>();
                    if (indicatorController != null)
                    {
                        indicatorController.SetValue(item.PurchasePrice, false);
                    }
                }
            }
        }

        private void ClearTags()
        {
            if (itemTagsText != null)
            {
                itemTagsText.text = "No details available. Try to research more about the item.";
            }
            if (tagDescription != null) tagDescription.text = "";
        }

        private void UpdateTagsDisplay(ItemModel item)
        {
            ClearTags();
            var currentDisplayedTags = item.Tags.FindAll(c => c.IsRevealedToPlayer);

            if (currentDisplayedTags == null || currentDisplayedTags.Count == 0)
            {
                return;
            }

            // Use helper to render tags with clickable links
            string formattedTags = "";
            for (int i = 0; i < currentDisplayedTags.Count; i++)
            {
                var tag = currentDisplayedTags[i];
                if (tag == null) continue;

                formattedTags += TagTextRenderHelper.RenderTag(tag);
                
                if (i < currentDisplayedTags.Count - 1)
                {
                    formattedTags += " ";
                }
            }
            itemTagsText.text = formattedTags;
        }



        private void OnTagsTextClicked()
        {
            // Find which tag was clicked based on mouse position
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(itemTagsText, Input.mousePosition, null);

            if (linkIndex != -1 && itemTagsText.textInfo != null && itemTagsText.textInfo.linkInfo != null)
            {
                if (linkIndex < itemTagsText.textInfo.linkInfo.Length)
                {
                    string linkId = itemTagsText.textInfo.linkInfo[linkIndex].GetLinkID();
                    OnTagLinkClicked(linkId);
                }
            }
        }

        private void OnTagLinkClicked(string linkId)
        {
            if (string.IsNullOrEmpty(linkId)) return;
            var prototype = _tagService.GetTagPrototypeByClassId(linkId);
            if (prototype != null)
            {
                tagDescription.text = !string.IsNullOrEmpty(prototype.Description) ? prototype.Description : $"No description available for {prototype.DisplayName}";
            }
        }
    }
}
