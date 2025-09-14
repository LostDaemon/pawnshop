using PawnShop.Services;
using PawnShop.Models.Characters;
using PawnShop.Models.Tags;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections;
using System.Linq;

namespace PawnShop.Controllers
{
    public class CustomerController : MonoBehaviour
    {
        [SerializeField] private Button _skipButton;
        [SerializeField] private SpriteRenderer _customerImage;
        [SerializeField] private SpriteRenderer _itemImage;
        [SerializeField] private SpriteMask _itemSpriteMask;
        [SerializeField] private GameObject _dirtLayer;
        [SerializeField] private GameObject _scratchesLayer;
        [SerializeField] private CanvasGroup _uiCanvasGroup;
        [SerializeField] private float _fadeDuration = 1.0f;

        private ICustomerService _customerService;
        private INegotiationService _negotiationService;

        [Inject]
        public void Construct(ICustomerService customerService, INegotiationService negotiationService)
        {
            _customerService = customerService;
            _negotiationService = negotiationService;
            _skipButton?.onClick.AddListener(OnSkipRequested);
            _customerService.OnCustomerChanged += OnCustomerChanged;
            _negotiationService.OnDealSuccess += OnSkipRequested;
            _negotiationService.OnNegotiationStarted += OnNegotiationStarted;
        }

        private void OnDestroy()
        {
            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveListener(OnSkipRequested);
            }

            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
                _negotiationService.OnDealSuccess -= OnSkipRequested;
                _negotiationService.OnNegotiationStarted -= OnNegotiationStarted;
            }
        }


        private void OnSkipRequested()
        {
            _customerService.ClearCustomer();
        }

        private void OnCustomerChanged(Customer customer)
        {
            Debug.Log($"[CustomerController] Customer changed to: {customer?.CustomerType}");

            if (customer == null)
            {
                // Customer is null, fade out
                HideAllLayers();
                StartCoroutine(FadeOut());
            }
            else
            {
                // Customer exists, fade in
                StartCoroutine(FadeIn());
            }
        }

        private void OnNegotiationStarted()
        {
            if (_negotiationService == null)
            {
                Debug.LogError("[CustomerController] NegotiationService is null in OnNegotiationStarted()");
                return;
            }

            var item = _negotiationService.CurrentItem;

            if (_itemImage != null && item != null)
            {
                if (item.Image != null)
                {
                    // Apply scale from item model to both sprite renderer and sprite mask
                    Vector3 itemScale = Vector3.one * item.Scale * 2;
                    
                    _itemImage.sprite = item.Image;
                    _itemImage.transform.localScale = itemScale;
                    
                    if (_itemSpriteMask != null)
                    {
                        _itemSpriteMask.sprite = item.Image;
                    }
                    
                    Debug.Log($"Item displayed: {item.Name} with scale: {item.Scale}");
                    
                    // Update visual layers based on item tags
                    UpdateVisualLayers(item);
                }
                else
                {
                    Debug.LogWarning($"Sprite not found for item: {item.Name}");
                }
            }
        }

        private IEnumerator FadeIn()
        {
            // Set initial alpha to 0
            SetAlpha(0f);

            float elapsedTime = 0f;

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / _fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            // Ensure final alpha is 1
            SetAlpha(1f);
        }

        private IEnumerator FadeOut()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            // Ensure final alpha is 0
            SetAlpha(0f);
        }

        private void SetAlpha(float alpha)
        {
            if (_customerImage != null)
            {
                Color color = _customerImage.color;
                color.a = alpha;
                _customerImage.color = color;
            }

            if (_itemImage != null)
            {
                Color color = _itemImage.color;
                color.a = alpha;
                _itemImage.color = color;
            }

            // Enable/disable SpriteMask based on alpha
            if (_itemSpriteMask != null)
            {
                _itemSpriteMask.enabled = alpha > 0f;
            }

            if (_uiCanvasGroup != null)
            {
                _uiCanvasGroup.alpha = alpha;
            }
        }

        private void UpdateVisualLayers(PawnShop.Models.ItemModel item)
        {
            // Hide all layers first
            HideAllLayers();

            if (item?.Tags == null) return;

            // Check for Dirt feature tag
            bool hasDirtTag = item.Tags.Any(tag => 
                tag.TagType == TagType.Feature && 
                tag.DisplayName == "Dirt");

            // Check for scratch feature tags
            bool hasScratchTag = item.Tags.Any(tag => 
                tag.TagType == TagType.Feature && 
                (tag.DisplayName == "Light Scratch" || tag.DisplayName == "Deep Scratch"));

            // Show appropriate layers
            if (hasDirtTag && _dirtLayer != null)
            {
                _dirtLayer.SetActive(true);
                Debug.Log($"[CustomerController] Showing DirtLayer for item: {item.Name}");
            }

            if (hasScratchTag && _scratchesLayer != null)
            {
                _scratchesLayer.SetActive(true);
                Debug.Log($"[CustomerController] Showing ScratchesLayer for item: {item.Name}");
            }
        }

        private void HideAllLayers()
        {
            if (_dirtLayer != null)
            {
                _dirtLayer.SetActive(false);
            }

            if (_scratchesLayer != null)
            {
                _scratchesLayer.SetActive(false);
            }
        }
    }
}
