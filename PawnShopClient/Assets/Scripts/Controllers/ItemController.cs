using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using PawnShop.Models;

public class ItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public ItemModel Payload { get; set; } //TODO: make generic
    public Transform CurrentParent { get; set; }
    public Image _image;

    public event Action<ItemModel> OnClick;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        CurrentParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        _image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        transform.SetParent(CurrentParent);
        transform.localPosition = Vector3.zero;
        _image.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[ItemController] OnPointerClick - Button: {eventData.button}, Payload: {Payload?.Name}");
        OnClick?.Invoke(Payload);
    }

    public void Init(ItemModel payload)
    {
        Payload = payload;
        
        // Update image if payload has image data
        if (payload?.Image != null && _image != null)
        {
            _image.sprite = payload.Image;
        }
    }
}
