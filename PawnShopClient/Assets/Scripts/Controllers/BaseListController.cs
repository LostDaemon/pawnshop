using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BaseListController : MonoBehaviour
{
    [SerializeField] private StorageType _sourceStorageType;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private BaseItemInfoController _itemInfo;
    private IGameStorageService<ItemModel> _storage;
    private IStorageLocatorService _storageLocatorService;
    private DiContainer _container;
    protected List<ListItemController> RenderedItems = new();
    public ItemModel SelectedItem;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService)
    {
        _storageLocatorService = storageLocatorService;
        _container = container;
        _storage = _storageLocatorService.Get(_sourceStorageType);
        Debug.Log($"Storage of type {_sourceStorageType} found: {_storage != null}");
        _storage.OnItemAdded += OnItemAdded;
        _storage.OnItemRemoved += OnItemRemoved;
    }

    private void Awake()
    {
        if (_storage == null)
        {
            Debug.LogError($"Storage of type {_sourceStorageType} not found.");
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

        foreach (var item in RenderedItems)
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
        controller.Init(item);
        RenderedItems.Add(controller);
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

    private void RemoveItem(ItemModel item)
    {
        var toDelete = RenderedItems.FirstOrDefault(c => c.Item == item);

        if (toDelete == null)
        {
            Debug.LogWarning($"Item {item.Name} not found in rendered items.");
            return;
        }

        RenderedItems.Remove(toDelete);
        toDelete.OnClick -= OnItemClicked;
        Destroy(toDelete.gameObject);
    }
}