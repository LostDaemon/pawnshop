using System;
using PawnShop.Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;

namespace PawnShop.Controllers
{
    public class ListItemController : MonoBehaviour
    {
        private Image _image;
        private ItemModel _item;

        public event Action<ItemModel> OnClick;
        public ItemModel Item => _item;


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

            if (_image == null || _item.Image == null)
            {
                Debug.LogWarning($"Sprite for item '{item.Name}' not found.");
                _image.sprite = null;
            }

            _image.sprite = _item.Image;
            _image.preserveAspect = true;
        }


        private StorageTypeMarkerController GetStorageTypeMarker()
        {
            var current = transform;
            while (current != null)
            {
                var marker = current.GetComponent<StorageTypeMarkerController>();
                if (marker != null)
                {
                    return marker;
                }
                current = current.parent;
            }
            return null;
        }
    }
}
