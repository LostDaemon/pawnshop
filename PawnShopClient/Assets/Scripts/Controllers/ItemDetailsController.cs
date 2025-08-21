using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

namespace PawnShop.Controllers
{
    public class ItemDetailsController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private IndicatorController itemPriceIndicator;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemTagsText;
        [SerializeField] private TextMeshProUGUI tagDescription;

        [Header("Settings")]
        [SerializeField] private Color fallbackTagColor = Color.black;

        private ISpriteService _spriteService;
        private ITagRepositoryService _tagRepository;

        [Inject]
        private void Construct(ISpriteService spriteService, ITagRepositoryService tagRepository)
        {
            _spriteService = spriteService;
            _tagRepository = tagRepository;
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

            var sprite = _spriteService.GetSprite(item.ImageId);

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

            string formattedTags = "";
            for (int i = 0; i < currentDisplayedTags.Count; i++)
            {
                var tag = currentDisplayedTags[i];
                if (tag == null) continue;

                // Create clickable link for each tag

                string tagColor = GetTagColorHex(tag);

                // Format as [TagType: DisplayName]
                string tagDisplayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
                string formattedTag = $"[{tag.TagType}: {tagDisplayName}]";
                formattedTags += $"<color=#{tagColor}><link=\"{tag.ClassId}\">{formattedTag}</link></color>";

                // Add space between tags
                if (i < currentDisplayedTags.Count - 1)
                {
                    formattedTags += " ";
                }
            }
            itemTagsText.text = formattedTags;
        }

        private string GetTagColorHex(BaseTagModel tag)
        {
            // Use the tag's own color if available, otherwise use fallback color
            if (tag != null && tag.Color != Color.clear)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(tag.Color);
                return colorHex;
            }

            string fallbackHex = ColorUtility.ToHtmlStringRGB(fallbackTagColor);
            return fallbackHex;
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
            var prototype = _tagRepository.GetTagPrototypeByClassId(linkId);
            if (prototype != null)
            {
                tagDescription.text = !string.IsNullOrEmpty(prototype.Description) ? prototype.Description : $"No description available for {prototype.DisplayName}";
            }
        }
    }
}
