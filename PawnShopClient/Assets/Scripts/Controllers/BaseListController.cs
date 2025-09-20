using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class BaseListController : MonoBehaviour
    {
        [SerializeField] private StorageType _sourceStorageType;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private BaseItemInfoController _itemInfo;
        private ISlotStorageService<ItemModel> _storage;
        private IStorageLocatorService _storageLocatorService;
        private DiContainer _container;
        protected List<ItemSlotController> RenderedItems = new();
        public ItemModel SelectedItem;

        [Inject]
        public void Construct(DiContainer container, IStorageLocatorService storageLocatorService)
        {

            _storageLocatorService = storageLocatorService;
            _container = container;
            _storage = _storageLocatorService.Get(_sourceStorageType);
            Debug.Log($"Storage of type {_sourceStorageType} found: {_storage != null}");

            // Clear all existing items and rebuild the list
            ClearAllItems();
            RebuildList();


            Debug.Log($"[BaseListController] Subscribed to storage events for {gameObject.name}");
        }

        private void Awake()
        {
            if (_storage == null)
            {
                Debug.LogError($"Storage of type {_sourceStorageType} not found.");
                return;
            }

           // RebuildList();
        }

        private void OnDestroy()
        {
            // Clear the list and destroy all children
            RenderedItems.Clear();
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
            {
                var child = _contentRoot.GetChild(i);
                Destroy(child.gameObject);
            }
        }


        private void RemoveItemFromSlot(ItemSlotController slotController)
        {
            var itemController = slotController.GetComponentInChildren<ItemController>();
            if (itemController != null)
            {
                itemController.OnClick -= OnItemClicked;
                Destroy(itemController.gameObject);
            }
        }

        private void OnItemClicked(ItemModel item)
        {
            if (item == null)
            {
                Debug.LogError("Item is null in OnItemClicked.");
                return;
            }

            SelectedItem = item;
            RenderItemInfo(item);
        }

        private void RenderItemInfo(ItemModel item)
        {
            if (_itemInfo != null)
            {
                _itemInfo.gameObject.SetActive(true);
                _itemInfo.SetItem(SelectedItem);
            }
        }

        private void ClearAllItems()
        {
            // Clear the list first
            RenderedItems.Clear();

            // Destroy all child objects in content root
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
            {
                var child = _contentRoot.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        private void RebuildList()
        {
            ClearAllItems();
            Debug.Log($"Rebuilding list for storage type {_sourceStorageType}");

            if (_storage == null)
            {
                Debug.LogError($"Storage of type {_sourceStorageType} not found.");
                return;
            }

            var slotsCount = _storage.GetTotalSlotsCount();
            Debug.Log($"Storage has {slotsCount} slots.");
            // Create slots for all positions in storage
            for (int slotId = 0; slotId < slotsCount; slotId++)
            {
                var slotController = _container.InstantiatePrefabForComponent<ItemSlotController>(_itemSlotPrefab, _contentRoot);
                if (slotController == null)
                {
                    Debug.LogError($"Failed to instantiate ItemSlotController for slot {slotId}");
                    continue;
                }
                slotController.Init(slotId, _sourceStorageType);
                RenderedItems.Add(slotController);

                // ItemSlotController will handle visual representation through events
                // No need to manually add items here
            }
        }
    }
}