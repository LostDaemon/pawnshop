using UnityEngine;
using UnityEngine.UI;

public class TradeCellController : MonoBehaviour
{

    [SerializeField] private GameObject _addButtonPrefab;
    public ItemModel Item { get; private set; }

    private Image _itemImageInstance;
    private GameObject _buttonInstance;
    private Canvas _uiCanvas;

    private static Sprite[] _cachedAtlas;

    private void Start()
    {
        if (_uiCanvas == null)
            _uiCanvas = FindFirstObjectByType<Canvas>();

        if (_itemImageInstance == null)
            _itemImageInstance = GetComponentInChildren<Image>(true);
        UpdateVisuals();
    }

    private void Update()
    {
        if (_buttonInstance != null && _buttonInstance.activeSelf)
            PositionButton();
    }

    public void SetItem(ItemModel item)
    {
        Item = item;
        UpdateVisuals();
    }

    public void ClearItem()
    {
        Item = null;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Item == null)
        {
            Debug.Log($"Clearing item in cell {name}");
            if (_itemImageInstance != null)
                _itemImageInstance.gameObject.SetActive(false);
            ShowButton();
        }
        else
        {
            Debug.Log($"Setting item {Item.Name} in cell {name}");

            HideButton();
            if (_itemImageInstance != null)
            {
                var sprite = LoadSprite(Item.ImageId);
                if (sprite != null)
                    _itemImageInstance.sprite = sprite;
                _itemImageInstance.gameObject.SetActive(true);
            }
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

    private void ShowButton()
    {
        if (_uiCanvas == null || _addButtonPrefab == null)
            return;

        if (_buttonInstance == null)
        {
            _buttonInstance = Instantiate(_addButtonPrefab, _uiCanvas.transform);
        }
        _buttonInstance.SetActive(true);
        PositionButton();
    }

    private void HideButton()
    {
        if (_buttonInstance != null)
            _buttonInstance.SetActive(false);
    }

    private void PositionButton()
    {

        if (_uiCanvas == null || _buttonInstance == null)
            return;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _uiCanvas.transform as RectTransform,
            screenPos,
            _uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _uiCanvas.worldCamera,
            out Vector2 localPos);

        _buttonInstance.GetComponent<RectTransform>().localPosition = localPos;
    }
}