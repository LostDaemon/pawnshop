using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TradeListController : MonoBehaviour
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _itemPrefab;

    private IGameStorageService<ItemModel> _sellStorage;
    private readonly Dictionary<ItemModel, TradeListItemController> _spawned = new();

    [Inject]
    public void Construct([Inject(Id = "SellStorage")] IGameStorageService<ItemModel> sellStorage)
    {
        _sellStorage = sellStorage;
        _sellStorage.OnItemAdded += OnItemAdded;
        _sellStorage.OnItemRemoved += OnItemRemoved;
    }

    private void Start()
    {
        foreach (var item in _sellStorage.All)
            AddItem(item);
    }

    private void OnDestroy()
    {
        if (_sellStorage != null)
        {
            _sellStorage.OnItemAdded -= OnItemAdded;
            _sellStorage.OnItemRemoved -= OnItemRemoved;
        }
    }

    private void OnItemAdded(ItemModel item) => AddItem(item);

    private void OnItemRemoved(ItemModel item) => RemoveItem(item);

    private void AddItem(ItemModel item)
    {
        if (_spawned.ContainsKey(item))
            return;
        var instance = Instantiate(_itemPrefab, _contentRoot);
        var controller = instance.GetComponent<TradeListItemController>();
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