using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TradeListController : MonoBehaviour
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _itemPrefab;

    private IGameStorageService<ItemModel> _inventoryStorage;
    private DiContainer _container;
    private readonly Dictionary<ItemModel, TradeListItemController> _spawned = new();

    [Inject]
    public void Construct(
        [Inject(Id = "Inventory")] IGameStorageService<ItemModel> inventoryStorage,
        DiContainer container)
    {
        _inventoryStorage = inventoryStorage;
        _container = container;
        _inventoryStorage.OnItemAdded += OnItemAdded;
        _inventoryStorage.OnItemRemoved += OnItemRemoved;
    }

    private void Start()
    {
        foreach (var item in _inventoryStorage.All)
            AddItem(item);
    }

    private void OnDestroy()
    {
        if (_inventoryStorage != null)
        {
            _inventoryStorage.OnItemAdded -= OnItemAdded;
            _inventoryStorage.OnItemRemoved -= OnItemRemoved;
        }
    }

    private void OnItemAdded(ItemModel item) => AddItem(item);

    private void OnItemRemoved(ItemModel item) => RemoveItem(item);

    private void AddItem(ItemModel item)
    {
        if (_spawned.ContainsKey(item))
            return;
        var controller = _container.InstantiatePrefabForComponent<TradeListItemController>(
            _itemPrefab, _contentRoot);
        if (controller != null)
            controller.Show(item);
        _spawned[item] = controller;
    }

    private void RemoveItem(ItemModel item)
    {
        if (_spawned.TryGetValue(item, out var controller) && controller != null)
        {
            Destroy(controller.gameObject);
            _spawned.Remove(item);
        }
    }
}