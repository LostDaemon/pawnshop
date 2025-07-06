using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class ListItemController : MonoBehaviour, IDraggable
{
    private Image _image;
    private ISpriteService _spriteService;
    private IDragAndDropService _dragAndDropService;
    private ItemModel _item;
    private DragAndDropContext _dragAndDropContext;
    public event Action<ItemModel> OnClick;
    public ItemModel Item => _item;

    public event Action<IDraggable, Vector2, DragAndDropContext> OnItemDrop;
    public event Action<DragAndDropContext> OnItemStartDragging;

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
        _item = item;
        var sprite = _spriteService.GetSprite(_item.ImageId);

        if (_image == null || sprite == null)
        {
            Debug.LogWarning($"Sprite for item '{item.Name}' with ID '{item.ImageId}' not found.");
            _image.sprite = null;
        }

        _image.sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Begin dragging item: {_item.Name}");

        _dragAndDropContext = new DragAndDropContext
        {
            Raycaster = GetComponentInParent<GraphicRaycaster>(),
            Canvas = GetComponentInParent<Canvas>(),
            CurrentPosition = eventData.position,
            InitialPosition = transform.position,
            InitialParent = transform.parent
        };

        OnItemStartDragging?.Invoke(_dragAndDropContext);

        transform.SetParent(transform.root); //TODO: Move to drag service
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Pointer down on item: {_item.Name}");
        OnClick?.Invoke(_item);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        if (_dragAndDropContext != null)
        {
            _dragAndDropContext.CurrentPosition = eventData.position;
            _dragAndDropService.UpdateContext(this, _dragAndDropContext);
        }
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        OnItemDrop?.Invoke(this, eventData.position, _dragAndDropContext);
    }
}
