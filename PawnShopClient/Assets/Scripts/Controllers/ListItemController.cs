using System;
using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PawnShop.Controllers
{
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

            if (_image == null || _item.Image == null)
            {
                Debug.LogWarning($"Sprite for item '{item.Name}' not found.");
                _image.sprite = null;
            }

            _image.sprite = _item.Image;
            _image.preserveAspect = true;
        }

        public void OnClicked()
        {
            Debug.Log($"Item clicked");
            OnClick?.Invoke(_item);
        }
    }
}
