using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ListItemController : MonoBehaviour
{
    private Image _image;
    private ISpriteService _spriteService;
    private ItemModel _item;

    public event Action<ItemModel> OnClick;
    public ItemModel Item => _item;

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

    public void Init(ItemModel item)
    {
        Debug.Log($"INIT: {item.Name}");
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
}
