using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class TradeListItemController : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private IndicatorController _priceIndicator;
    [SerializeField] private Button _sellButton;

    private static Sprite[] _cachedAtlas;

    private IStorageRouterService<ItemModel> _router;
    private IGameStorageService<ItemModel> _inventory;
    private IGameStorageService<ItemModel> _sellStorage;
    private ISellService _sellService;

    private ItemModel _item;

    [Inject]
    public void Construct(
        IStorageRouterService<ItemModel> router,
        [Inject(Id = "Inventory")] IGameStorageService<ItemModel> inventory,
        [Inject(Id = "SellStorage")] IGameStorageService<ItemModel> sellStorage,
        ISellService sellService)
    {
        _router = router;
        _inventory = inventory;
        _sellStorage = sellStorage;
        _sellService = sellService;
    }

    private void Awake()
    {
        if (_sellButton != null)
            _sellButton.onClick.AddListener(OnSellClicked);
    }

    private void OnDestroy()
    {
        if (_sellButton != null)
            _sellButton.onClick.RemoveListener(OnSellClicked);
    }

    public void Show(ItemModel item)
    {
        _item = item;

        if (_title != null)
            _title.text = item.Name;

        if (_priceIndicator != null)
            _priceIndicator.SetValue(item.RealPrice, animate: false);

        if (_image != null)
        {
            var sprite = LoadSprite(item.ImageId);
            if (sprite != null)
                _image.sprite = sprite;
            else
                Debug.LogWarning($"Sprite '{item.ImageId}' not found in atlas.");
        }
    }

    private Sprite LoadSprite(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (_cachedAtlas == null)
            _cachedAtlas = Resources.LoadAll<Sprite>("Sprites/ItemsAtlas");

        return System.Array.Find(_cachedAtlas, s => s.name == id);
    }

    private void OnSellClicked()
    {
        Debug.Log($"Selling item: {_inventory.All.Count}");

        if (_item == null)
            return;

        _router.Transfer(_item, _inventory, _sellStorage);

        _sellService?.TryAutoFillDisplay();
    }
}