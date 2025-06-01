using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SellExpositionController : MonoBehaviour
{
    [SerializeField] private Transform[] _anchors;
    [SerializeField] private GameObject _itemLotPrefab;

    private readonly Dictionary<ItemModel, GameObject> _activeLots = new();
    private ISellService _sellService;

    [Inject]
    public void Construct(ISellService sellService)
    {
        _sellService = sellService;
        _sellService.ConfigureSlots(_anchors.Length);
        _sellService.OnDisplayUpdated += RefreshDisplay;
    }

    private void OnDestroy()
    {
        if (_sellService != null)
            _sellService.OnDisplayUpdated -= RefreshDisplay;
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        // Удаляем все старые объекты
        foreach (var go in _activeLots.Values)
            Destroy(go);

        _activeLots.Clear();

        var items = _sellService.DisplayedItems;
        for (int i = 0; i < items.Count && i < _anchors.Length; i++)
        {
            var item = items[i];
            var anchor = _anchors[i];

            var instance = Instantiate(_itemLotPrefab, anchor.position, Quaternion.identity, anchor);
            var lot = instance.GetComponent<ItemLotController>();
            lot.Initialize(item, anchor);

            _activeLots[item] = instance;
        }
    }
}