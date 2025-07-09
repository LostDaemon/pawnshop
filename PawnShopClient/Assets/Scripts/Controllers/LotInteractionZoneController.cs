using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LotInteractionZoneController : MonoBehaviour
{
    private int _lotId;
    private Image _image;
    public int LotIndex
    {
        get => _lotId;
        set
        {
            _lotId = value;
            Refresh();
        }
    }

    private void Refresh()
    {
        _image.sprite = null;
        if (_storage == null || _storage.All.Count <= _lotId)
        {
            return;
        }

        var item = _storage.All[_lotId];
        if (item != null)
        {
            _image.sprite = _spriteService.GetSprite(item.ImageId);
        }
    }

    private IStorageLocatorService _storageLocatorService;
    private ISpriteService _spriteService;
    private IGameStorageService<ItemModel> _storage;
    [Inject]
    public void Construct(IStorageLocatorService storageLocatorService, ISpriteService spriteService)
    {
        _spriteService = spriteService;
        _storageLocatorService = storageLocatorService;
        _storage = _storageLocatorService.Get(StorageType.SellStorage);
        _storage.OnItemAdded += OnItemAdded;
        _storage.OnItemRemoved += OnItemRemoved;
    }

    private void OnDestroy()
    {
        if (_storage != null)
        {
            _storage.OnItemAdded -= OnItemAdded;
            _storage.OnItemRemoved -= OnItemRemoved;
        }
    }

    private void OnItemRemoved(ItemModel model)
    {
        Refresh();
    }

    private void OnItemAdded(ItemModel model)
    {
        Refresh();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _image = GetComponent<Image>();
        Debug.Assert(_image != null, "Image component is not assigned in LotInteractionZoneController.");
        if (_image == null)
        {
            Debug.LogError("Image component not found on LotInteractionZoneController.");
            return;
        }

        if (_storage == null)
        {
            Debug.LogError($"Storage of type {StorageType.SellStorage} not found.");
            return;
        }
    }


}
