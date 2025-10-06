using UnityEngine;

namespace PawnShop.Controllers
{
    /// <summary>
    /// NPC Visual Controller - applies random colors and scales to sprites
    /// </summary>
    public class NpcVisualController : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField] private bool grayscale = true;
        
        [Header("Scale Settings")]
        [SerializeField] private float minScale = 0.9f;
        [SerializeField] private float maxScale = 1.1f;
        
        [Header("Layer Settings")]
        [SerializeField] private int minOrderInLayer = 101;
        [SerializeField] private int maxOrderInLayer = 200;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private void Awake()
        {
            // Auto-find SpriteRenderer if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = GetComponentInParent<SpriteRenderer>();
                }
            }
        }
        
        private void Start()
        {
            InitializeRandomVisuals();
        }
        
        /// <summary>
        /// Initialize random color, scale and layer order for sprite
        /// </summary>
        private void InitializeRandomVisuals()
        {
            if (spriteRenderer == null) return;
            
            // Set random color
            if (grayscale)
            {
                // Generate random gray value
                float grayValue = Random.Range(0.3f, 0.8f);
                Color randomGray = new Color(grayValue, grayValue, grayValue, 1f);
                spriteRenderer.color = randomGray;
            }
            else
            {
                // Generate random RGB color
                Color randomColor = new Color(
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    Random.Range(0f, 1f),
                    1f
                );
                spriteRenderer.color = randomColor;
            }
            
            // Set random scale
            float randomScale = Random.Range(minScale, maxScale);
            transform.localScale = new Vector3(randomScale, randomScale, 1f);
            
            // Set random order in layer
            int randomOrderInLayer = Random.Range(minOrderInLayer, maxOrderInLayer + 1);
            spriteRenderer.sortingOrder = randomOrderInLayer;
        }
        
        /// <summary>
        /// Set custom color
        /// </summary>
        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
        
        /// <summary>
        /// Get current color
        /// </summary>
        public Color GetColor()
        {
            return spriteRenderer != null ? spriteRenderer.color : Color.white;
        }
    }
}
