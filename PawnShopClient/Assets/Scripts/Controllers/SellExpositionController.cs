using UnityEngine;
using Zenject;

public class SellExpositionController : MonoBehaviour
{
    [SerializeField] private TradeCellController[] _cells;


    private ISellService _sellService;

    [Inject]
    public void Construct(ISellService sellService)
    {
        _sellService = sellService;
        _sellService.ConfigureSlots(_cells.Length);
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
        foreach (var cell in _cells)
            cell.ClearItem();

        var items = _sellService.DisplayedItems;
        for (int i = 0; i < items.Count && i < _cells.Length; i++)
        {
            var item = items[i];
            _cells[i].SetItem(item);
        }
    }
}