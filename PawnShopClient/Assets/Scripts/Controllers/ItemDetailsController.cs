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
        [SerializeField] private TextMeshProUGUI TagDescription;

        [Header("Settings")]
        [SerializeField] private Color fallbackTagColor = Color.black;

        private ItemModel currentItem;
        private Dictionary<string, Action> tagClickHandlers;
        private ISpriteService spriteService;
        private INegotiationService negotiationService;
        private ITagRepositoryService tagRepository;
        private List<BaseTagModel> currentDisplayedTags;

        [Inject]
        private void Construct(ISpriteService spriteService, INegotiationService negotiationService, ITagRepositoryService tagRepository)
        {
            this.spriteService = spriteService;
            this.negotiationService = negotiationService;
            this.tagRepository = tagRepository;
            tagClickHandlers = new Dictionary<string, Action>();
            currentDisplayedTags = new List<BaseTagModel>();
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

            // Subscribe to negotiation service events
            if (negotiationService != null)
            {
                negotiationService.OnCurrentItemChanged += OnNegotiationItemChanged;
                negotiationService.OnCurrentOfferChanged += OnNegotiationOfferChanged;
                negotiationService.OnTagsRevealed += OnTagsRevealed;
            }
        }

        private void OnTagsRevealed(List<BaseTagModel> tags)
        {
            UpdateTagsDisplay(tags);
        }

        /// <summary>
        /// Display item details in the UI
        /// </summary>
        /// <param name="item">Item to display</param>
        public void DisplayItem(ItemModel item)
        {
            if (item == null) return;

            currentItem = item;
            UpdateUI();
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void ClearDisplay()
        {
            currentItem = null;
            ClearUI();
        }

        /// <summary>
        /// Register a click handler for a specific tag
        /// </summary>
        /// <param name="tagId">Tag identifier</param>
        /// <param name="handler">Action to execute when tag is clicked</param>
        public void RegisterTagClickHandler(string tagId, Action handler)
        {
            if (handler != null)
            {
                tagClickHandlers[tagId] = handler;
            }
        }

        /// <summary>
        /// Unregister a click handler for a specific tag
        /// </summary>
        /// <param name="tagId">Tag identifier</param>
        public void UnregisterTagClickHandler(string tagId)
        {
            if (tagClickHandlers.ContainsKey(tagId))
            {
                tagClickHandlers.Remove(tagId);
            }
        }

        private void UpdateUI()
        {
            if (currentItem == null) return;

            // Update item name
            if (itemNameText != null)
            {
                itemNameText.text = currentItem.Name;
            }

            // Update item description
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = currentItem.Description;
            }

            // Update item price
            if (itemPriceIndicator != null)
            {
                itemPriceIndicator.SetValue(currentItem.BasePrice, false);
            }

            // Update item image
            if (itemImage != null)
            {
                var sprite = spriteService.GetSprite(currentItem.ImageId);
                if (sprite != null)
                {
                    itemImage.sprite = sprite;
                    itemImage.enabled = true;
                    itemImage.preserveAspect = true;
                }
                else
                {
                    Debug.LogWarning($"Sprite '{currentItem.ImageId}' not found for item '{currentItem.Name}'");
                    itemImage.enabled = false;
                }
            }

            ClearTags();
            // Final check - ensure rich text is enabled
            if (itemTagsText != null && !itemTagsText.richText)
            {
                Debug.LogError("[ItemDetailsController] Rich text is still disabled after UpdateTagsDisplay!");
            }
        }

        private void ClearTags()
        {
            if (itemTagsText != null)
            {
                itemTagsText.text = "No visible tags";
            }
            if (TagDescription != null) TagDescription.text = "";
            currentDisplayedTags.Clear();
        }

        private void UpdateTagsDisplay(List<BaseTagModel> tags)
        {
            // Store the current displayed tags for click handling
            currentDisplayedTags = tags ?? new List<BaseTagModel>();

            if (currentDisplayedTags.Count == 0)
            {
                ClearTags();
                return;
            }

            string formattedTags = "";
            for (int i = 0; i < currentDisplayedTags.Count; i++)
            {
                var tag = currentDisplayedTags[i];
                if (tag == null) continue;

                // Create clickable link for each tag
                string tagId = $"tag_{i}";
                string tagColor = GetTagColorHex(tag);

                // Format as [TagType: DisplayName]
                string tagDisplayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
                string formattedTag = $"[{tag.TagType}: {tagDisplayName}]";
                formattedTags += $"<color=#{tagColor}><link=\"{tagId}\">{formattedTag}</link></color>";

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
                Debug.Log($"[ItemDetailsController] Using tag color: {tag.Color} -> #{colorHex}");
                return colorHex;
            }

            string fallbackHex = ColorUtility.ToHtmlStringRGB(fallbackTagColor);
            Debug.Log($"[ItemDetailsController] Using fallback color: {fallbackTagColor} -> #{fallbackHex}");
            return fallbackHex;
        }

        private void ClearUI()
        {
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescriptionText != null) itemDescriptionText.text = "";
            if (itemPriceIndicator != null) itemPriceIndicator.SetValue(0, false);
            if (itemImage != null) itemImage.enabled = false;
            if (itemTagsText != null) itemTagsText.text = "";
            if (TagDescription != null) TagDescription.text = "";
            currentDisplayedTags.Clear();
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

            // Check if we have a registered handler for this tag
            if (tagClickHandlers.TryGetValue(linkId, out Action handler))
            {
                handler?.Invoke();
            }
            else
            {
                // Default behavior - log the click
                Debug.Log($"Tag clicked: {linkId}");

                // You can add default tag handling logic here
                HandleDefaultTagClick(linkId);
            }
        }

        private void HandleDefaultTagClick(string linkId)
        {
            // Extract tag index from linkId (format: "tag_X")
            if (linkId.StartsWith("tag_") && int.TryParse(linkId.Substring(4), out int tagIndex))
            {
                if (currentDisplayedTags != null && tagIndex < currentDisplayedTags.Count)
                {
                    var clickedTag = currentDisplayedTags[tagIndex];
                    string tagName = !string.IsNullOrEmpty(clickedTag.DisplayName) ? clickedTag.DisplayName : clickedTag.TagType.ToString();
                    Debug.Log($"Default tag click handler: {tagName}");

                    // Get tag prototype from repository by ClassId to get full description
                    var tagPrototype = tagRepository.GetTagPrototypeByClassId(clickedTag.ClassId);
                    if (tagPrototype != null)
                    {
                        // Display tag description from prototype in TagDescription TMP
                        if (TagDescription != null)
                        {
                            string description = !string.IsNullOrEmpty(tagPrototype.Description) ? tagPrototype.Description : $"No description available for {tagName}";
                            TagDescription.text = description;
                        }
                    }
                    else
                    {
                        // Fallback to tag model description if prototype not found
                        if (TagDescription != null)
                        {
                            string description = !string.IsNullOrEmpty(clickedTag.Description) ? clickedTag.Description : $"No description available for {tagName}";
                            TagDescription.text = description;
                        }
                    }

                    // Add your default tag handling logic here
                    // For example, show tooltip, highlight related items, etc.
                }
            }
        }

        private void OnNegotiationItemChanged(ItemModel item)
        {
            // Update the display when negotiation service changes the current item
            DisplayItem(item);
        }

        private void OnNegotiationOfferChanged(long newOffer)
        {
            // Update the price display when the current offer changes
            if (itemPriceIndicator != null)
            {
                itemPriceIndicator.SetValue(newOffer, true);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from negotiation service events
            if (negotiationService != null)
            {
                negotiationService.OnCurrentItemChanged -= OnNegotiationItemChanged;
                negotiationService.OnCurrentOfferChanged -= OnNegotiationOfferChanged;
                negotiationService.OnTagsRevealed -= OnTagsRevealed;
            }
        }
    }
}
