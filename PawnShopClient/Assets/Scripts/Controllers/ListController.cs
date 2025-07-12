using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

public class ListController : MonoBehaviour
{
    [SerializeField] private StorageType _sourceStorageType;
    [SerializeField] private StorageType _targetStorageType;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private BaseItemInfoController _itemInfo;
    private IGameStorageService<ItemModel> _storage;
    private IStorageLocatorService _storageLocatorService;
    private ISellService _sellService;
    private DiContainer _container;
    private List<ListItemController> _renderedItems = new();
    private ItemModel _selectedItem;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, ISellService sellService)
    {
        _storageLocatorService = storageLocatorService;
        _sellService = sellService;
        _container = container;
        _storage = _storageLocatorService.Get(_sourceStorageType);
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
        controller.Init(item);
        _renderedItems.Add(controller);
    }

    private void OnItemClicked(ItemModel item)
    {
        if (item == null)
        {
            Debug.LogError("Item is null in OnItemClicked.");
            return;
        }

        _selectedItem = item;
        RenderItemInfo(item);
    }

    private void RenderItemInfo(ItemModel item)
    {
        if (_itemInfo != null)
        {
            _itemInfo.gameObject.SetActive(true);
            _itemInfo.SetItem(_selectedItem);
        }
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

    public void Schedule()
    {
        if (_selectedItem == null)
        {
            return;
        }

        _selectedItem.SellPrice = (int)(_selectedItem.PurchasePrice * 1.1f);
        _sellService.ScheduleForSale(_selectedItem);
        _selectedItem = null;
    }
}