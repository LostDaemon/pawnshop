using UnityEngine;
using Zenject;

public class ItemDisplayController : MonoBehaviour
{
    [SerializeField] private Transform _parent;

    [Inject] private IPurchaseService _purchaseService;

    private ItemController _current;

    private void Start()
    {
        if (_purchaseService == null)
        {
            Debug.LogError("PurchaseService is not injected.");
            return;
        }

        _purchaseService.OnCurrentItemChanged += OnItemChanged;

        if (_purchaseService.CurrentItem != null)
        {
            OnItemChanged(_purchaseService.CurrentItem);
        }
    }

    private void OnDestroy()
    {
        if (_purchaseService != null)
            _purchaseService.OnCurrentItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(ItemModel item)
    {
        if (_current != null)
            Destroy(_current.gameObject);

        var prefab = Resources.Load<ItemController>("UI/ItemPrefab");
        _current = Instantiate(prefab, _parent);
        _current.Show(item);

        Debug.Log($"Item displayed: {item.Name}");
    }
}