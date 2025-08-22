using PawnShop.Models;
using UnityEngine;

namespace PawnShop.Controllers
{
    public class ItemLotController : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        public ItemModel Item { get; private set; }
        public Transform Anchor { get; private set; }

        public System.Action<ItemModel> OnClicked;

        public void Initialize(ItemModel item, Transform anchor)
        {
            Item = item;
            Anchor = anchor;

            if (_renderer == null)
                _renderer = GetComponentInChildren<SpriteRenderer>();

            if (_renderer == null)
            {
                Debug.LogError("SpriteRenderer not found on ItemLot prefab.");
                return;
            }

            if (item.Image != null)
                _renderer.sprite = item.Image;
            else
                Debug.LogWarning($"Sprite not found for item: {item.Name}");

            transform.localScale = Vector3.one * item.Scale * 0.5f;
        }



        private void OnMouseDown()
        {
            OnClicked?.Invoke(Item);
        }
    }
}