using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Services;
using Zenject;
using UnityEngine.EventSystems;
using PawnShop.Repositories;
using System.Reflection;

namespace PawnShop.Controllers
{
    public class ItemInfoController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _currentOfferText;
        [SerializeField] private TextMeshProUGUI _purchasePriceText;
        [SerializeField] private TextMeshProUGUI _tagList;

        private ItemModel _item;
        private ILocalizationService _localizationService;
        private ITagRepository _tagRepository;

        [Inject]
        public void Construct(ILocalizationService localizationService, ITagRepository tagRepository)
        {
            _localizationService = localizationService;
            _tagRepository = tagRepository;
            _localizationService.OnLocalizationSwitch += OnLocalizationChanged;
        }

        private void Awake()
        {
            // Enable rich text support for tags text
            if (_tagList != null)
            {
                _tagList.richText = true;

                // Add click event handler to tags text
                var eventTrigger = _tagList.gameObject.GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = _tagList.gameObject.AddComponent<EventTrigger>();
                }

                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => OnTagsTextClicked());
                eventTrigger.triggers.Add(entry);
            }
        }

        private void OnDestroy()
        {
            if (_localizationService != null)
            {
                _localizationService.OnLocalizationSwitch -= OnLocalizationChanged;
            }
        }

        private void OnTagsTextClicked()
        {
            // Find which tag was clicked based on mouse position
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tagList, Input.mousePosition, null);

            if (linkIndex != -1 && _tagList.textInfo != null && _tagList.textInfo.linkInfo != null)
            {
                if (linkIndex < _tagList.textInfo.linkInfo.Length)
                {
                    string linkId = _tagList.textInfo.linkInfo[linkIndex].GetLinkID();
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
                Debug.Log($"[ItemInfoController] Tag clicked: {prototype.TagType} - {prototype.Description}");
            }
        }

        private void OnLocalizationChanged()
        {
            // Update tags when localization changes
            if (_item != null)
            {
                UpdateTags();
            }
        }

        public void SetItem(ItemModel item)
        {
            _item = item;
            OnItemChanged();
        }

        private void OnItemChanged()
        {
            if (_item == null)
            {
                ClearUI();
                return;
            }

            UpdateItemImage();
            UpdateTitle();
            UpdateDescription();
            UpdateCurrentOffer();
            UpdatePurchasePrice();
            UpdateTags();
        }

        private void UpdateItemImage()
        {
            if (_itemImage != null && _item.Image != null)
            {
                _itemImage.sprite = _item.Image;
                _itemImage.preserveAspect = true;
            }
        }

        private void UpdateTitle()
        {
            if (_titleText != null)
            {
                _titleText.text = _item.Name ?? string.Empty;
            }
        }

        private void UpdateDescription()
        {
            if (_descriptionText != null)
            {
                _descriptionText.text = _item.Description ?? string.Empty;
            }
        }

        private void UpdateCurrentOffer()
        {
            if (_currentOfferText != null)
            {
                _currentOfferText.text = _item.CurrentOffer.ToString();
            }
        }

        private void UpdatePurchasePrice()
        {
            if (_purchasePriceText != null)
            {
                _purchasePriceText.text = _item.PurchasePrice.ToString();
            }
        }

        private void UpdateTags()
        {
            if (_item?.Tags == null || _tagList == null)
                return;

            const string DEFAULT_ICON = "\uf005";
            var tagTexts = new System.Collections.Generic.List<string>();

            foreach (var tag in _item.Tags)
            {
                if (tag.IsRevealedToPlayer)
                {
                    // Get localized text
                    string displayText = tag.DisplayName;
                    if (_localizationService != null)
                    {
                        displayText = _localizationService.GetLocalization(displayText);
                    }

                    // Get icon
                    string icon = !string.IsNullOrEmpty(tag.Icon) ? tag.Icon : DEFAULT_ICON;
                    
                    // Convert Unicode string to character using Regex.Unescape
                    Debug.Log($"Converted icon: '{icon}' (length: {icon.Length})");
 
                    // Create clickable colored text with icon (no space to prevent line breaks)
                    string colorHex = ColorUtility.ToHtmlStringRGB(tag.Color != Color.clear ? tag.Color : Color.white);
                    string tagText = $"<color=#{colorHex}><link=\"{tag.ClassId}\">{icon} {displayText}</link></color>";

                    tagTexts.Add(tagText);
                }
            }

            // Join all tags with comma and space
            _tagList.text = string.Join(", ", tagTexts);
        }

        private void ClearUI()
        {
            if (_itemImage != null)
            {
                _itemImage.sprite = null;
            }

            if (_titleText != null)
            {
                _titleText.text = string.Empty;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = string.Empty;
            }

            if (_currentOfferText != null)
            {
                _currentOfferText.text = string.Empty;
            }

            if (_purchasePriceText != null)
            {
                _purchasePriceText.text = string.Empty;
            }

            if (_tagList != null)
            {
                _tagList.text = string.Empty;
            }
        }
    }
}
