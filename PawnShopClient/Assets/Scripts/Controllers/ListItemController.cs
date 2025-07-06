using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class ListItemController : MonoBehaviour, IDraggable
{
    private Image _image;
    private ISpriteService _spriteService;
    private ItemModel _item;
    public StorageType StorageType { get; private set; }
    public event Action<ItemModel> OnClick;
    public event Action<IDraggable, PointerEventData> OnItemDrag;
    public event Action<IDraggable, PointerEventData> OnItemEndDrag;
    public event Action<IDraggable, PointerEventData> OnItemBeginDrag;
    public ItemModel Payload => _item;


    [Inject]
    public void Construct(ISpriteService spriteService)
    {
        _spriteService = spriteService;
    }

    private void Awake()
    {
        _image = GetComponentInChildren<Image>();
        if (_image == null)
        {
            Debug.LogError("Image component not found on ListItemController.");
            return;
        }
    }

    public void Init(StorageType storageType, ItemModel item)
    {
        StorageType = storageType;
        _item = item;

        var sprite = _spriteService.GetSprite(_item.ImageId);

        if (_image == null || sprite == null)
        {
            Debug.LogWarning($"Sprite for item '{item.Name}' with ID '{item.ImageId}' not found.");
            _image.sprite = null;
        }

        _image.sprite = sprite;
    }

    public void OnClicked()
    {
        Debug.Log($"Item clicked");
        OnClick?.Invoke(_item);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnItemBeginDrag?.Invoke(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnItemDrag?.Invoke(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnItemEndDrag?.Invoke(this, eventData);
    }
}
