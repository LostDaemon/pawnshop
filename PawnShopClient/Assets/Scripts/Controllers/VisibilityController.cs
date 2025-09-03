using System.Collections;
using UnityEngine;

namespace PawnShop.Controllers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class VisibilityController : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.3f;
        
        private CanvasGroup canvasGroup;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetVisibility(bool visible, bool instant = false)
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
