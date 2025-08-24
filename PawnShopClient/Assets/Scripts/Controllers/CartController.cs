using System.Collections;
using UnityEngine;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class CartController : MonoBehaviour
    {
        [Header("Cart Type")]
        [SerializeField] private CartType _cartType = CartType.Undefined;
        
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer[] _shell;
        
        [Header("Animation")]
        [SerializeField] private float _fadeSpeed = 2f;
        
        private Coroutine _fadeCoroutine;
        
        /// <summary>
        /// Makes shell sprites transparent to show cart contents
        /// </summary>
        public void Show()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            
            _fadeCoroutine = StartCoroutine(FadeSprites(0f));
        }
        
        /// <summary>
        /// Makes shell sprites opaque to hide cart contents
        /// </summary>
        public void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            
            _fadeCoroutine = StartCoroutine(FadeSprites(1f));
        }
        
        private IEnumerator FadeSprites(float targetAlpha)
        {
            if (_shell == null || _shell.Length == 0) yield break;
            
            // Get current alpha from first sprite (assuming all have same alpha)
            float startAlpha = _shell[0].color.a;
            
            float elapsedTime = 0f;
            float duration = Mathf.Abs(targetAlpha - startAlpha) / _fadeSpeed;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
                
                // Apply alpha to all shell sprites
                foreach (var sprite in _shell)
                {
                    if (sprite != null)
                    {
                        Color color = sprite.color;
                        color.a = currentAlpha;
                        sprite.color = color;
                    }
                }
                
                yield return null;
            }
            
            // Ensure final alpha value is exact
            foreach (var sprite in _shell)
            {
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = targetAlpha;
                    sprite.color = color;
                }
            }
            
            _fadeCoroutine = null;
        }
        
        /// <summary>
        /// Instantly sets all sprites to target alpha without animation
        /// </summary>
        public void SetAlphaInstantly(float alpha)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            
            foreach (var sprite in _shell)
            {
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = Mathf.Clamp01(alpha);
                    sprite.color = color;
                }
            }
        }
        
        /// <summary>
        /// Returns true if fade animation is currently running
        /// </summary>
        public bool IsAnimating => _fadeCoroutine != null;
        
        /// <summary>
        /// Returns the type of this cart
        /// </summary>
        public CartType CartType => _cartType;
    }
}
