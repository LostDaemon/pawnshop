using System.Collections;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Controllers
{
    public class ParallaxLayerController : MonoBehaviour
    {
        public Vector2 parallaxFactor = Vector2.one;
        [SerializeField] private float moveDuration = 0.5f;

        private Vector3 currentOffset;
        private Coroutine moveCoroutine;

        public void Initialize(INavigationService navService)
        {
            currentOffset = Vector3.zero;
            navService.OnWorldPositionChanged += OnMove;
        }

        private void OnMove(Vector3 newPos)
        {
            Vector3 desiredOffset = new Vector3(
                -newPos.x * parallaxFactor.x,
                -newPos.y * parallaxFactor.y,
                0f
            );

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(SmoothMove(transform.position, desiredOffset));
            currentOffset = desiredOffset;
        }

        private IEnumerator SmoothMove(Vector3 from, Vector3 to)
        {
            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                transform.position = Vector3.Lerp(from, to, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = to;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}