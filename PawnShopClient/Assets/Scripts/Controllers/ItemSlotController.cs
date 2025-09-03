using System;
using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class ItemSlotController : SlotControllerBase
    {
        [SerializeField] private GameObject _coverImage;
        [SerializeField] private GameObject _selectionMarker;
        [SerializeField] public SpriteRenderer _spriteRenderer;

        private IGameStorageService<ItemModel> _sellStorageService;
        private IStorageLocatorService _storageLocatorService;
        private IShelfService _shelfService;
        private ItemModel _currentItem;
        private bool _isEnabled = true;

        public event Action<ItemModel> OnTransferCancelled;
        
        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set 
            { 
                _isEnabled = value; 
                UpdateCoverVisibility();
            } 
        }

        [Inject]
        public void Construct(IStorageLocatorService storageLocatorService, IShelfService shelfService)
        {
            _storageLocatorService = storageLocatorService;
            _shelfService = shelfService;
            _sellStorageService = _storageLocatorService.Get(StorageType.SellStorage);
        }

        protected override void OnAwake()
        {
            // Auto-assign SpriteRenderer if not set in inspector
            if (_spriteRenderer == null)
            {
                var itemImageChild = transform.Find("item_image");
                _spriteRenderer = itemImageChild?.GetComponent<SpriteRenderer>();
            }

            // Auto-assign cover image if not set
            if (_coverImage == null)
            {
                _coverImage = transform.Find("cover_image")?.gameObject;
            }

            // Auto-assign selection marker if not set
            if (_selectionMarker == null)
            {
                _selectionMarker = transform.Find("selection_marker")?.gameObject;
            }

            if (_spriteRenderer != null)
            {
                // Initialize slot as transparent
                var color = _spriteRenderer.color;
                color.a = 0f;
                _spriteRenderer.color = color;
            }

            // Initialize cover visibility
            UpdateCoverVisibility();
        }

        protected override void OnInteraction()
        {
            if (!IsEnabled)
            {
                return;
            }
        }

        private StorageTypeMarkerController GetStorageTypeMarker()
        {
            var current = transform;
            while (current != null)
            {
                var marker = current.GetComponent<StorageTypeMarkerController>();
                if (marker != null)
                {
                    return marker;
                }
                current = current.parent;
            }
            return null;
        }
        
        private void UpdateCoverVisibility()
        {
            if (_coverImage != null)
            {
                _coverImage.SetActive(!IsEnabled);
            }
        }

        private void ShowSelectionMarker()
        {
            if (_selectionMarker != null)
            {
                _selectionMarker.SetActive(true);
            }
        }

        private void HideSelectionMarker()
        {
            if (_selectionMarker != null)
            {
                _selectionMarker.SetActive(false);
            }
        }

        private void SetSlotItem(ItemModel item)
        {
            _currentItem = item;
            HideSelectionMarker();
            SetItemImage(item);
        }

        private void SetItemImage(ItemModel item)
        {
            if (item == null || _spriteRenderer == null)
            {
                return;
            }

            if (item.Image != null)
            {
                _spriteRenderer.sprite = item.Image;
                
                var color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;

                if (item.Scale != 0)
                {
                    _spriteRenderer.transform.localScale = Vector3.one * item.Scale;
                }
                else
                {
                    _spriteRenderer.transform.localScale = Vector3.one;
                }
            }
            else
            {
                ClearSlot();
            }
        }

        private void ClearSlot()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = null;
                _spriteRenderer.transform.localScale = Vector3.one;
                
                // Set alpha to 0 (fully transparent) when clearing slot
                var color = _spriteRenderer.color;
                color.a = 0f;
                _spriteRenderer.color = color;
            }
        }

        private void OnDestroy()
        {
            // Unregister mapping from ShelfService
            _shelfService.UnregisterMapping(Id);
        }
    }
}
