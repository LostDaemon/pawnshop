using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Linq;
using PawnShop.Models;
using PawnShop.Services;
using PawnShop.Repositories;
using Zenject;

public class ItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public ItemModel Payload { get; set; } //TODO: make generic
    public Transform CurrentParent { get; set; }
    public Image _image;
    public Image _scratches;
    public Image _dirt;
    public Image _destroyed;
    public Image _mask;
    [SerializeField] private GameObject _processIcon;

    private ITagService _tagService;

    public event Action<ItemModel> OnClick;

    [Inject]
    public void Construct(ITagService tagService)
    {
        _tagService = tagService;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CurrentParent = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Disable raycast for all images
        if (_image != null) _image.raycastTarget = false;
        if (_scratches != null) _scratches.raycastTarget = false;
        if (_dirt != null) _dirt.raycastTarget = false;
        if (_destroyed != null) _destroyed.raycastTarget = false;
        if (_mask != null) _mask.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(CurrentParent);
        transform.localPosition = Vector3.zero;

        // Enable raycast for all images
        if (_image != null) _image.raycastTarget = true;
        if (_scratches != null) _scratches.raycastTarget = true;
        if (_dirt != null) _dirt.raycastTarget = true;
        if (_destroyed != null) _destroyed.raycastTarget = true;
        if (_mask != null) _mask.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(Payload);
    }

    public void Init(ItemModel payload)
    {
        Payload = payload;

        // Update image if payload has image data
        if (payload?.Image != null && _image != null)
        {
            _image.sprite = payload.Image;
            _image.transform.localScale = Vector3.one * payload.Scale;
        }

        // Set mask to same texture as image
        if (payload?.Image != null && _mask != null)
        {
            _mask.sprite = payload.Image;
            _mask.transform.localScale = Vector3.one * payload.Scale;
        }

        // Update visual layers based on item tags
        UpdateVisualLayers(payload);
    }

    public void UpdateVisualLayers(ItemModel item)
    {
        // Hide all layers first
        HideAllLayers();

        if (item?.Tags == null || _tagService == null) return;


        // Get default tag prototypes
        var lightScratchPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.FeatureLightScratch);
        var deepScratchPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.FeatureDeepScratch);
        var dirtPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.FeatureDirt);
        var destroyedPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.ConditionDestroyed);

        // Check for scratch feature tags
        bool hasScratchTag = item.Tags.Any(tag =>
            (lightScratchPrototype != null && tag.ClassId == lightScratchPrototype.ClassId) ||
            (deepScratchPrototype != null && tag.ClassId == deepScratchPrototype.ClassId));

        // Check for Dirt feature tag
        bool hasDirtTag = dirtPrototype != null &&
            item.Tags.Any(tag => tag.ClassId == dirtPrototype.ClassId);

        // Check for Destroyed condition tag
        bool hasDestroyedTag = destroyedPrototype != null &&
            item.Tags.Any(tag => tag.ClassId == destroyedPrototype.ClassId);

        // Show appropriate layers
        if (hasScratchTag && _scratches != null)
        {
            _scratches.gameObject.SetActive(true);
        }

        if (hasDirtTag && _dirt != null)
        {
            _dirt.gameObject.SetActive(true);
        }

        if (hasDestroyedTag && _destroyed != null)
        {
            _destroyed.gameObject.SetActive(true);
        }
    }

    public void ShowProcess(bool isActive)
    {
        if (_processIcon != null)
        {
            _processIcon.SetActive(isActive);
        }
    }

    private void HideAllLayers()
    {
        if (_scratches != null)
        {
            _scratches.gameObject.SetActive(false);
        }

        if (_dirt != null)
        {
            _dirt.gameObject.SetActive(false);
        }

        if (_destroyed != null)
        {
            _destroyed.gameObject.SetActive(false);
        }
    }
}
