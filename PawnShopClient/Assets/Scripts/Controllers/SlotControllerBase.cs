using UnityEngine;
using System;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class SlotControllerBase : MonoBehaviour
    {
        [SerializeField] private string id = "";
        [SerializeField] private ItemModel item;
        
        private Camera mainCamera;
        private Collider2D objectCollider;
        
        public string Id => id;
        public ItemModel Item 
        { 
            get => item; 
            set => item = value; 
        }
        public bool IsEmpty => item == null;
        
        protected virtual void OnAwake()
        {
            // Override in derived classes for custom initialization
        }
        
        protected virtual void OnUpdate()
        {
            // Override in derived classes for custom update logic
        }

        protected virtual void OnInteraction()
        {
            // Override in derived classes for custom interaction logic
        }
        
        private void Awake()
        {
            // Ensure we have a collider for raycasting
            objectCollider = GetComponent<Collider2D>();
            if (objectCollider == null)
            {
                Debug.LogError($"SlotControllerBase on {gameObject.name} requires a Collider2D component!");
            }
            
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found!");
            }
            
            OnAwake();
        }

        private void Update()
        {
            // Check for mouse click or touch
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                CheckForInteraction();
            }
            
            OnUpdate();
        }

        private void CheckForInteraction()
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            
            // Check if mouse position overlaps with this object's collider
            if (objectCollider.OverlapPoint(mousePosition))
            {
                HandleInteraction();
            }
        }

        private void HandleInteraction()
        {
            OnInteraction();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Generate ID when component is first added (only in editor)
            if (string.IsNullOrEmpty(id))
            {
                id = $"slot_{Guid.NewGuid()}";
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Generated new ID for {gameObject.name}: {id}");
            }
        }

        private void OnValidate()
        {
            // Ensure ID is set (only in editor, only for prefab instances)
            if (string.IsNullOrEmpty(id) && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this) == UnityEditor.PrefabInstanceStatus.Connected)
            {
                id = $"slot_{Guid.NewGuid()}";
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Generated new ID for {gameObject.name}: {id}");
            }
        }
#endif
    }
}
