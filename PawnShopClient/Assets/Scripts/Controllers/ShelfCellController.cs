using UnityEngine;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public class ShelfCellController : MonoBehaviour
    {
        [SerializeField] private StorageType _storageType;
        [SerializeField] private int _slotId;
        [SerializeField] private GameObject _uiPrefab;
        [SerializeField] private Transform _parent;
        
        private DiContainer _container;
        private GameObject _instantiatedUIPrefab;
        private ItemSlotController _itemSlotController;
        private WorldspaceToCanvasProjectionController _worldspaceToCanvasController;
        
        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }
        
        private void Start()
        {
            // Auto-initialize if parameters are set in inspector
            if (_storageType != StorageType.Undefined && _uiPrefab != null)
            {
                Initialize(_storageType, _slotId, _uiPrefab, _parent);
            }
            else
            {
                Debug.LogWarning($"[ShelfCellController] {gameObject.name} is not properly configured. Please set StorageType and UIPrefab in inspector.");
            }
        }
        
        public void Initialize(StorageType storageType, int slotId, GameObject uiPrefab, Transform parent = null)
        {
            _storageType = storageType;
            _slotId = slotId;
            _uiPrefab = uiPrefab;
            
            InstantiateUIPrefab(parent);
            SetupControllers();
        }
        
        private void InstantiateUIPrefab(Transform parent = null)
        {
            if (_uiPrefab == null)
            {
                Debug.LogError("[ShelfCellController] UIPrefab is null, cannot instantiate");
                return;
            }
            
            // Use provided parent or fallback to this controller's transform
            Transform targetParent = parent != null ? parent : transform;
            
            // Instantiate the UI prefab as a child of the specified parent
            _instantiatedUIPrefab = _container.InstantiatePrefab(_uiPrefab, targetParent);
            
            if (_instantiatedUIPrefab == null)
            {
                Debug.LogError("[ShelfCellController] Failed to instantiate UI prefab");
                return;
            }
            
            Debug.Log($"[ShelfCellController] Successfully instantiated UI prefab for slot {_slotId} under parent {(parent != null ? parent.name : "self")}");
        }
        
        private void SetupControllers()
        {
            if (_instantiatedUIPrefab == null)
            {
                Debug.LogError("[ShelfCellController] Cannot setup controllers - instantiated UI prefab is null");
                return;
            }
            
            // Get ItemSlotController from the instantiated prefab
            _itemSlotController = _instantiatedUIPrefab.GetComponentInChildren<ItemSlotController>();
            if (_itemSlotController == null)
            {
                Debug.LogError("[ShelfCellController] ItemSlotController not found in instantiated UI prefab");
                return;
            }
            
            // Initialize ItemSlotController with the parameters
            _itemSlotController.Init(_slotId, _storageType);
            
            // Get WorldspaceToCanvasProjectionController from the instantiated prefab
            _worldspaceToCanvasController = _instantiatedUIPrefab.GetComponentInChildren<WorldspaceToCanvasProjectionController>();
            if (_worldspaceToCanvasController == null)
            {
                Debug.LogWarning("[ShelfCellController] WorldspaceToCanvasProjectionController not found in instantiated UI prefab");
            }
            else
            {
                // Set this transform as the world space object for projection
                _worldspaceToCanvasController.SetWorldSpaceObject(transform);
            }
            
            Debug.Log($"[ShelfCellController] Successfully setup controllers for slot {_slotId} with storage type {_storageType}");
        }
        
        public void UpdateVisual()
        {
            if (_itemSlotController != null)
            {
                _itemSlotController.UpdateVisual();
            }
        }
        
        public ItemSlotController GetItemSlotController()
        {
            return _itemSlotController;
        }
        
        public WorldspaceToCanvasProjectionController GetWorldspaceToCanvasController()
        {
            return _worldspaceToCanvasController;
        }
        
        public StorageType GetStorageType()
        {
            return _storageType;
        }
        
        public int GetSlotId()
        {
            return _slotId;
        }
        
        private void OnDestroy()
        {
            // Clean up instantiated prefab
            if (_instantiatedUIPrefab != null)
            {
                Destroy(_instantiatedUIPrefab);
            }
        }
    }
}
