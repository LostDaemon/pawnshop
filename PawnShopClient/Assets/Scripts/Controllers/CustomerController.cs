using PawnShop.Services;
using PawnShop.Models.Characters;
using PawnShop.Models.Tags;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using System.Collections;
using System.Linq;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class CustomerController : MonoBehaviour
    {
        [SerializeField] private Button _skipButton;
        [SerializeField] private SpriteRenderer _customerImage;
        [SerializeField] private SpriteRenderer _customerFace;
        [SerializeField] private ItemController _itemController;
        [SerializeField] private CanvasGroup _uiCanvasGroup;
        [SerializeField] private float _fadeDuration = 1.0f;

        private ICustomerService _customerService;
        private INegotiationService _negotiationService;
        private ITimeService _timeService;

        [Inject]
        public void Construct(ICustomerService customerService, INegotiationService negotiationService, ITimeService timeService)
        {
            _customerService = customerService;
            _negotiationService = negotiationService;
            _timeService = timeService;
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
            if (customer == null)
            {
                // Customer is null, fade out and deactivate ItemController
                if (_itemController != null)
                {
                    _itemController.gameObject.SetActive(false);
                }
                StartCoroutine(FadeOut());
            }
            else
            {
                // Customer exists, fade in and activate ItemController
                if (_itemController != null)
                {
                    _itemController.gameObject.SetActive(true);
                }
                StartCoroutine(FadeIn());
            }
        }

        private void OnNegotiationStarted()
        {
            if (_negotiationService == null)
            {
                return;
            }

            var item = _negotiationService.CurrentItem;

            if (item != null)
            {
                // Initialize ItemController with customer's item
                if (_itemController != null)
                {
                    _itemController.Init(item);
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

            if (_customerFace != null)
            {
                Color color = _customerFace.color;
                color.a = alpha;
                _customerFace.color = color;
            }

            if (_uiCanvasGroup != null)
            {
                _uiCanvasGroup.alpha = alpha;
            }
        }

    }
}
