using System.Collections;
using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenUIController : MonoBehaviour
    {
        [SerializeField] private CartType cartType;
        [SerializeField] private float fadeDuration = 0.3f;

        private CanvasGroup canvasGroup;
        private NavigationService navigationService;
        private Coroutine fadeRoutine;

        [Inject]
        private void Construct(NavigationService navigation)
        {
            navigationService = navigation;
            canvasGroup = GetComponent<CanvasGroup>();

            navigationService.OnCartChanged += HandleCartChanged;

            Debug.Log($"[ScreenUIController2] Construct - Current cart: {navigationService.CurrentCart}, my cart type: {cartType}");
            bool initialVisibility = navigationService.CurrentCart == cartType;
            Debug.Log($"[ScreenUIController2] Initial visibility: {initialVisibility}");
            SetVisibility(initialVisibility, instant: true);
        }

        public void Initialize(NavigationService navigation)
        {
            navigationService = navigation;
            canvasGroup = GetComponent<CanvasGroup>();

            navigationService.OnCartChanged += HandleCartChanged;
            SetVisibility(navigationService.CurrentCart == cartType, instant: true);
        }

        private void OnDestroy()
        {
            if (navigationService != null)
                navigationService.OnCartChanged -= HandleCartChanged;
        }

        private void HandleCartChanged(CartType newCart)
        {
            Debug.Log($"[ScreenUIController2] Cart changed to: {newCart}, my cart type: {cartType}");
            bool shouldShow = newCart == cartType;
            Debug.Log($"[ScreenUIController2] Should show: {shouldShow}");
            SetVisibility(shouldShow, instant: false);
        }

        private void SetVisibility(bool visible, bool instant)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            if (instant)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                fadeRoutine = StartCoroutine(FadeTo(visible ? 1f : 0f));
            }
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = time / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            bool visible = targetAlpha > 0.9f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

    }
}
