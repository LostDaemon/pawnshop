using System.Collections.Generic;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class ParallaxController : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [SerializeField] private Color spriteColor = Color.white;
        [SerializeField] private Sprite[] availableSprites;
        [SerializeField] private Vector3 spriteScale = Vector3.one;
        [SerializeField] private int layerOrder = 0;
        [SerializeField] private float maxDistance = 10f;
        [SerializeField] private float slowdownCoefficient = 1f;
        [SerializeField] private float baseSpeed = 2f;
        
        [Inject] private ITrainStateService _trainStateService;
        
        private List<GameObject> spriteObjects;
        private Transform parentTransform;
        
        private void Start()
        {
            InitializeParallax();
            SubscribeToTrainEvents();
        }
        
        private void InitializeParallax()
        {
            if (availableSprites == null || availableSprites.Length == 0)
            {
                Debug.LogError("ParallaxController: No sprites provided!");
                return;
            }
            
            spriteObjects = new List<GameObject>();
            parentTransform = transform;
            
            // Get all child objects and convert them to sprites
            SetupChildSprites();
        }
        
        private void SetupChildSprites()
        {
            // Get all child objects
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform child = parentTransform.GetChild(i);
                GameObject spriteObj = child.gameObject;
                
                // Add SpriteRenderer if not present
                SpriteRenderer spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
                }
                
                // Set random sprite and color
                spriteRenderer.sprite = GetRandomSprite();
                spriteRenderer.color = spriteColor;
                spriteRenderer.sortingOrder = layerOrder;
                
                // Set scale
                spriteObj.transform.localScale = spriteScale;
                
                // Keep original local position - don't reset to zero
                
                spriteObjects.Add(spriteObj);
            }
        }
        
        private void Update()
        {
            MoveSprites();
            CheckAndRecycleSprites();
        }
        
        private void MoveSprites()
        {
            // Get speed from train service if available, otherwise use base speed
            float currentSpeed = _trainStateService != null ? _trainStateService.CurrentSpeed : baseSpeed;
            float movement = currentSpeed * slowdownCoefficient * Time.deltaTime;
            
            foreach (GameObject spriteObj in spriteObjects)
            {
                if (spriteObj != null)
                {
                    Vector3 currentPos = spriteObj.transform.localPosition;
                    spriteObj.transform.localPosition = new Vector3(currentPos.x + movement, 0f, 0f);
                }
            }
        }
        
        private void CheckAndRecycleSprites()
        {
            for (int i = spriteObjects.Count - 1; i >= 0; i--)
            {
                GameObject spriteObj = spriteObjects[i];
                
                if (spriteObj == null)
                {
                    spriteObjects.RemoveAt(i);
                    continue;
                }
                
                float distanceFromParent = Mathf.Abs(spriteObj.transform.localPosition.x);
                
                if (distanceFromParent > maxDistance)
                {
                    // Destroy the sprite that went too far
                    Destroy(spriteObj);
                    spriteObjects.RemoveAt(i);
                    
                    // Create new sprite on the opposite side
                    CreateNewSpriteOnOppositeSide(spriteObj.transform.localPosition.x);
                }
            }
        }
        
        private void CreateNewSprite()
        {
            GameObject newSpriteObj = new GameObject($"ParallaxSprite_{spriteObjects.Count}");
            newSpriteObj.transform.SetParent(parentTransform);
            
            // Spawn at maxDistance on the right side
            newSpriteObj.transform.localPosition = new Vector3(maxDistance, 0f, 0f);
            
            SpriteRenderer spriteRenderer = newSpriteObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetRandomSprite();
            spriteRenderer.color = spriteColor;
            spriteRenderer.sortingOrder = layerOrder;
            
            // Set scale
            newSpriteObj.transform.localScale = spriteScale;
            
            spriteObjects.Add(newSpriteObj);
        }
        
        private void CreateNewSpriteOnOppositeSide(float oldSpriteX)
        {
            GameObject newSpriteObj = new GameObject($"ParallaxSprite_{spriteObjects.Count}");
            newSpriteObj.transform.SetParent(parentTransform);
            
            // Spawn on the opposite side at maxDistance
            float spawnX = oldSpriteX > 0 ? -maxDistance : maxDistance;
            newSpriteObj.transform.localPosition = new Vector3(spawnX, 0f, 0f);
            
            SpriteRenderer spriteRenderer = newSpriteObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetRandomSprite();
            spriteRenderer.color = spriteColor;
            spriteRenderer.sortingOrder = layerOrder;
            
            // Set scale
            newSpriteObj.transform.localScale = spriteScale;
            
            spriteObjects.Add(newSpriteObj);
        }
        
        private Sprite GetRandomSprite()
        {
            if (availableSprites.Length == 0) return null;
            return availableSprites[Random.Range(0, availableSprites.Length)];
        }
        
        public void SetColor(Color newColor)
        {
            spriteColor = newColor;
            UpdateAllSpriteColors();
        }
        
        public void SetSprites(Sprite[] newSprites)
        {
            availableSprites = newSprites;
        }
        
        public void SetScale(Vector3 newScale)
        {
            spriteScale = newScale;
            UpdateAllSpriteScales();
        }
        
        public void SetLayerOrder(int newLayerOrder)
        {
            layerOrder = newLayerOrder;
            UpdateAllSpriteLayerOrders();
        }
        
        public void SetMaxDistance(float newMaxDistance)
        {
            maxDistance = newMaxDistance;
        }
        
        public void SetSlowdownCoefficient(float newCoefficient)
        {
            slowdownCoefficient = newCoefficient;
        }
        
        public void SetSpeed(float newSpeed)
        {
            baseSpeed = newSpeed;
        }
        
        private void UpdateAllSpriteColors()
        {
            foreach (GameObject spriteObj in spriteObjects)
            {
                if (spriteObj != null)
                {
                    SpriteRenderer spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = spriteColor;
                    }
                }
            }
        }
        
        private void UpdateAllSpriteScales()
        {
            foreach (GameObject spriteObj in spriteObjects)
            {
                if (spriteObj != null)
                {
                    spriteObj.transform.localScale = spriteScale;
                }
            }
        }
        
        private void UpdateAllSpriteLayerOrders()
        {
            foreach (GameObject spriteObj in spriteObjects)
            {
                if (spriteObj != null)
                {
                    SpriteRenderer spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = layerOrder;
                    }
                }
            }
        }

        private void SubscribeToTrainEvents()
        {
            if (_trainStateService != null)
            {
                _trainStateService.OnSpeedChanged += OnTrainSpeedChanged;
            }
        }

        private void OnTrainSpeedChanged(float newSpeed)
        {
            // Parallax will automatically use the new speed in the next MoveSprites call
            // This method can be used for additional effects if needed
        }

        private void OnDestroy()
        {
            if (_trainStateService != null)
            {
                _trainStateService.OnSpeedChanged -= OnTrainSpeedChanged;
            }
            StopAllCoroutines();
        }
    }
}
