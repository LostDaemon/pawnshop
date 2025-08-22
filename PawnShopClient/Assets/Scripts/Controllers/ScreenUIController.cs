using System.Collections;
using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Controllers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenUIController : MonoBehaviour
    {
        [SerializeField] private ScreenId screenId; // <- выбирается в инспекторе
        [SerializeField] private float fadeDuration = 0.3f;

        private CanvasGroup canvasGroup;
        private INavigationService navigationService;
        private Coroutine fadeRoutine;

        public void Initialize(INavigationService navigation)
        {
            navigationService = navigation;
            canvasGroup = GetComponent<CanvasGroup>();

            navigationService.OnScreenChanged += HandleScreenChanged;

            // Применить начальное состояние
            SetVisibility(navigationService.CurrentScreen == screenId, instant: true);
        }

        private void OnDestroy()
        {
            if (navigationService != null)
                navigationService.OnScreenChanged -= HandleScreenChanged;
        }

        private void HandleScreenChanged(ScreenId newScreen)
        {
            bool shouldShow = newScreen == screenId;
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