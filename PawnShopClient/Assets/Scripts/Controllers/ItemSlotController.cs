using UnityEngine;
using UnityEngine.EventSystems;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;

public class ItemSlotController : MonoBehaviour, IDropHandler
{
    [SerializeField] private StorageType _sourceStorageType;
    [SerializeField] private int _slotId;
    [SerializeField] private GameObject _itemPrefab;


    private IStorageLocatorService _storageLocatorService;
    private ISlotStorageService<ItemModel> _storageService;
    private DiContainer _container;
    private bool _isInitialized = false;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService)
    {
        _storageLocatorService = storageLocatorService;
        _container = container;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        var droppedObject = eventData.pointerDrag;
        Debug.Log($"Dropped Object: {(droppedObject != null ? droppedObject.name : "None")}");

        if (droppedObject != null)
        {
            var draggableItem = droppedObject.GetComponent<ItemController>();
            Debug.Log($"Draggable Item: {(draggableItem != null ? "Found" : "Not Found")}");
            if (draggableItem != null)
            {
                // Get the payload from DraggableItemController
                var itemModel = draggableItem.Payload as ItemModel;
                if (itemModel != null)
                {
                    HandleItemDrop(itemModel, draggableItem);
                }
            }
        }
    }

    private void HandleItemDrop(ItemModel itemModel, ItemController draggableItem)
    {

        if (!_isInitialized)
        {
            Init(_slotId, _sourceStorageType);
        }

        Debug.Log($"[InventorySlotController] Handling item drop: {itemModel?.Name} to slot {_slotId}, storage {_sourceStorageType}");

        // Check if slot is already occupied
        if (_storageService.HasItem(_slotId))
        {
            Debug.LogWarning($"[InventorySlotController] Slot {_slotId} is already occupied. Drop rejected.");
            return;
        }

        // Add item to this slot
        _storageService.Put(_slotId, itemModel);
        Debug.Log($"[InventorySlotController] Added item {itemModel.Name} to slot {_slotId} in storage {_sourceStorageType}");

        // Update DraggableItemController parent
        var previousSlot = draggableItem.CurrentParent.GetComponent<ItemSlotController>();
        if (previousSlot != null)
        {
            previousSlot.InformDropSuccess();
        }

        draggableItem.CurrentParent = this.transform;
    }

    public void Init(int slotId, StorageType sourceStorageType)
    {
        _slotId = slotId;
        _sourceStorageType = sourceStorageType;
        _storageService = _storageLocatorService.Get(_sourceStorageType);
        
        // Subscribe to storage changes for this specific slot
        if (_storageService != null)
        {
            _storageService.OnItemChanged += OnStorageItemChanged;
        }
        
        _isInitialized = true;
    }


    public void InformDropSuccess()
    {
        if (!_isInitialized)
        {
            Init(_slotId, _sourceStorageType);
        }
        _storageService.Withdraw(_slotId);
    }

    public void UpdateVisual()
    {
        if (!_isInitialized)
        {
            Init(_slotId, _sourceStorageType);
        }

        // Clear existing visual representation
        ClearSlot();

        var itemModel = _storageService.Get(_slotId);
        if (itemModel != null)
        {
            var itemObject = Instantiate(_itemPrefab, transform);
            var item = itemObject.GetComponent<ItemController>();
            item.Init(itemModel);
        }
    }

    private void OnStorageItemChanged(int slotId)
    {
        // Only react to changes in this specific slot
        if (slotId == _slotId)
        {
            Debug.Log($"[ItemSlotController] Item changed in slot {_slotId}");
            UpdateVisual();
        }
    }

    private void ClearSlot()
    {
        // Remove all child objects (item representations)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from storage events to prevent memory leaks
        if (_storageService != null)
        {
            _storageService.OnItemChanged -= OnStorageItemChanged;
        }
    }
}