using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class ListController : MonoBehaviour
{
    [SerializeField] private StorageType _storageType;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private TMP_Text _title;
    private IGameStorageService<ItemModel> _storage;
    IStorageLocatorService _storageLocatorService;
    private DiContainer _container;
    private IDragAndDropService _dragAndDropService;
    private List<ListItemController> _renderedItems = new();

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, IDragAndDropService dragAndDropService)
    {
        _storageLocatorService = storageLocatorService;
        _dragAndDropService = dragAndDropService;
        _container = container;
        _storage = _storageLocatorService.Get(_storageType);
        _storage.OnItemAdded += OnItemAdded;
        _storage.OnItemRemoved += OnItemRemoved;
    }

    private void Awake()
    {
        if (_storage == null)
        {
            Debug.LogError($"Storage of type {_storageType} not found.");
            return;
        }

        foreach (var item in _storage.All)
            AddItem(item);
    }

    private void OnDestroy()
    {
        if (_storage != null)
        {
            _storage.OnItemAdded -= OnItemAdded;
            _storage.OnItemRemoved -= OnItemRemoved;
        }

        foreach (var item in _renderedItems)
        {
            UnsubscribeItemEvents(item);
            Destroy(item.gameObject);
        }
    }

    private void OnItemAdded(ItemModel item) => AddItem(item);
    private void OnItemRemoved(ItemModel item) => RemoveItem(item);

    private void AddItem(ItemModel item)
    {
        var controller = _container.InstantiatePrefabForComponent<ListItemController>(_itemPrefab, _contentRoot);
        if (controller == null)
        {
            Debug.LogError($"Failed to instantiate ItemController for item {item.Name}");
            return;
        }

        controller.OnClick += OnItemClicked;
        controller.OnItemBeginDrag += OnItemBeginDrag;
        controller.OnItemDrag += OnItemDrag;
        controller.OnItemEndDrag += OnItemEndDrag;

        controller.Init(_storageType, item);
        _renderedItems.Add(controller);
    }

    private void OnItemEndDrag(IDraggable draggable, PointerEventData data)
    {
        throw new NotImplementedException();
    }

    private void OnItemDrag(IDraggable draggable, PointerEventData data)
    {
        //_dragAndDropService.Drag
    }

    private void OnItemBeginDrag(IDraggable draggable, PointerEventData data)
    {
        _dragAndDropService.StartDrag(_storage, draggable.Payload);
    }

    private void OnItemClicked(ItemModel item)
    {
        if (item == null)
        {
            Debug.LogError("Item is null in OnItemClicked.");
            return;
        }
        _title.text = item.Name;
        Debug.Log($"Item clicked: {item.Name}");
    }

    private void RemoveItem(ItemModel item)
    {
        var toDelete = _renderedItems.FirstOrDefault(c => c.Payload == item);

        if (toDelete == null)
        {
            Debug.LogWarning($"Item {item.Name} not found in rendered items.");
            return;
        }

        _renderedItems.Remove(toDelete);
        UnsubscribeItemEvents(toDelete);
        Destroy(toDelete.gameObject);
    }

    private void UnsubscribeItemEvents(ListItemController item)
    {
        item.OnClick -= OnItemClicked;
        item.OnItemBeginDrag -= OnItemBeginDrag;
        item.OnItemDrag -= OnItemDrag;
        item.OnItemEndDrag -= OnItemEndDrag;
    }
}