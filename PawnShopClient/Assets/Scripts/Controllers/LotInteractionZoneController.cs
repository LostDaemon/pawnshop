using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LotInteractionZoneController : MonoBehaviour, IDropTarget
{
    private Image _image;
    private ISpriteService _spriteService;
    private ItemModel _item;
    [Inject]
    public void Construct(ISpriteService spriteService)
    {
        _spriteService = spriteService;

        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }

        if (_image == null)
        {
            Debug.LogError("Image component not found on LotInteractionButton.");
            return;
        }
    }

    public bool TryDrop(IDraggable item)
    {
        if (_item != null)
        {
            return false;
        }

        var itemController = item as ListItemController;

        if (itemController == null)
        {
            Debug.LogWarning("Dropped item is not of type ItemModel.");
            return false;
        }

        _item = itemController.Item;
        var sprite = _spriteService.GetSprite(_item.ImageId);
        _image.sprite = sprite;
        return true;
    }

    private void Awake()
    {
        _image = GetComponentInChildren<Image>();
    }
}
