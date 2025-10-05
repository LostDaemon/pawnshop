using PawnShop.Controllers.DragNDrop;
using PawnShop.Models.Tags;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardSlotController : DragSlotController<BaseTagModel>
{
    [SerializeField] private Color _activeColor = Color.white;

    private Image _image;
    private Color _originalColor;

    private void Start()
    {
        _image = GetComponent<Image>();
        if (_image != null)
            _originalColor = _image.color;
    }


    private void Update()
    {
        if (_image != null)
        {
            if (canReceiveDragged)
            {
                _image.color = _activeColor;
            }
            else
            {
                _image.color = _originalColor;
            }
        }
    }

    protected override void OnItemDropped(DraggableItemController<BaseTagModel> draggableItem, PointerEventData eventData)
    {
        base.OnItemDropped(draggableItem, eventData);
        // Additional custom logic can be added here if needed
    }
}
