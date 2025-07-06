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
    private IStorageLocatorService _storageLocatorService;
    private IDragAndDropService _dragAndDropService;
    private DiContainer _container;
    private List<ListItemController> _renderedItems = new();

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, IDragAndDropService dragAndDropService)
    {
        _storageLocatorService = storageLocatorService;
        _dragAndDropService = dragAndDropService;
        _dragAndDropService.OnDragAndDropSucceed += OnDragAndDropSucceed;
        _container = container;
        _storage = _storageLocatorService.Get(_storageType);
        _storage.OnItemAdded += OnItemAdded;
        _storage.OnItemRemoved += OnItemRemoved;
    }

    private void OnDragAndDropSucceed(DragAndDropContext context)
    {

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
            item.OnClick -= OnItemClicked;
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
        controller.OnDrag += OnItemDrag;


        controller.Init(item);
        _renderedItems.Add(controller);
    }

    private void OnItemDrag(PointerEventData data)
    {
        throw new NotImplementedException();
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
        var toDelete = _renderedItems.FirstOrDefault(c => c.Item == item);

        if (toDelete == null)
        {
            Debug.LogWarning($"Item {item.Name} not found in rendered items.");
            return;
        }

        _renderedItems.Remove(toDelete);
        toDelete.OnClick -= OnItemClicked;
        Destroy(toDelete.gameObject);
    }
}