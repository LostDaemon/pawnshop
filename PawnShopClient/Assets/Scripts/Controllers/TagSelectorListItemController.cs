using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PawnShop.Models.Tags;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public class TagSelectorListItemController : MonoBehaviour
    {
        private TMP_Text _tagText;
        private Toggle _toggle;
        private BaseTagModel _tag;
        private ILocalizationService _localizationService;

        public Toggle Toggle => _toggle;
        public BaseTagModel Tag => _tag;

        [Inject]
        public void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        private void Awake()
        {
            // Get components without tree binding
            _tagText = GetComponentInChildren<TMP_Text>();
            _toggle = GetComponentInChildren<Toggle>();

            if (_tagText == null)
                Debug.LogError("TagSelectorListItemController: TMP_Text component not found.");

            if (_toggle == null)
                Debug.LogError("TagSelectorListItemController: Toggle component not found.");
        }

        /// <summary>
        /// Initialize the list item with tag data
        /// </summary>
        /// <param name="tag">Tag model to display</param>
        public void Init(BaseTagModel tag)
        {
            _tag = tag;

            if (_tagText != null)
            {
                _tagText.text = GetTagDisplayText(tag);
                _tagText.color = tag.Color != Color.clear ? tag.Color : Color.white;
            }

            if (_toggle != null)
            {
                _toggle.isOn = false; // Start unchecked
            }
        }

        private string GetTagDisplayText(BaseTagModel tag)
        {
            if (tag == null) return "Unknown Tag";

            var displayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
            
            // Apply localization if service is available
            if (_localizationService != null)
            {
                displayName = _localizationService.GetLocalization(displayName);
            }
            
            // Get icon
            string icon = !string.IsNullOrEmpty(tag.Icon) ? tag.Icon : "\uf005"; // Default FontAwesome star

            // Add specific value for different tag types
            return $"{icon} {displayName}";
        }

        /// <summary>
        /// Set the toggle state
        /// </summary>
        /// <param name="isOn">Whether the toggle should be on</param>
        public void SetToggleState(bool isOn)
        {
            if (_toggle != null)
            {
                _toggle.isOn = isOn;
            }
        }

        /// <summary>
        /// Get the current toggle state
        /// </summary>
        /// <returns>Current toggle state</returns>
        public bool GetToggleState()
        {
            return _toggle != null && _toggle.isOn;
        }
    }
}
