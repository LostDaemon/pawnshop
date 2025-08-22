using PawnShop.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace PawnShop.Controllers
{
    public class ItemInfoController : BaseItemInfoController
    {
        private TMP_Text _infoText;
        private Image _image;
        private ISpriteService _spriteService;

        [Inject]
        public void Construct(ISpriteService spriteService)
        {
            _spriteService = spriteService;
        }

        protected override void OnAwake()
        {
            _infoText = GetComponentInChildren<TMP_Text>();
            _image = GetComponentInChildren<Image>();

            if (_infoText == null)
                Debug.LogError("SellItemInfoController: TMP_Text component not found.");

            if (_image == null)
                Debug.LogError("SellItemInfoController: Image component not found.");
        }

        protected override void OnItemChanged()
        {
            if (_infoText != null && Item != null)
                _infoText.text = Item.Name;

            if (Item?.Inspected == true)
            {
                _infoText.text += $"\n{Item.Inspected}";
            }


            if (_image != null && Item != null)
            {


                var sprite = _spriteService.GetSprite(Item.ImageId);

                if (sprite != null)
                {
                    _image.sprite = sprite;
                    _image.preserveAspect = true;
                }
                else
                {
                    Debug.LogWarning($"SellItemInfoController: Sprite '{Item.ImageId}' not found.");
                    _image.sprite = null;
                }
            }
            else
            {
                Debug.LogWarning("SellItemInfoController: Item or Image component is null.");
            }
        }
    }
}