using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PawnShop.Controllers.DragNDrop
{
    public abstract class DraggableItemController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] public bool canDrag = true;
        public Image image;
        [HideInInspector] public Transform parentAfterDrag;
        private Image[] childImages;

        protected virtual void Awake()
        {
            // Find all Image components in children
            childImages = GetComponentsInChildren<Image>();
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
            
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            SetRaycastTarget(false);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
            
            transform.position = eventData.position;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
            
            Debug.Log($"[{GetType().Name}] OnEndDrag - Item: {gameObject.name}, Position: {eventData.position}");
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;

            // Reset RectTransform to fill the container after setting parent
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.offsetMin = Vector2.zero; // Left, Bottom
                rectTransform.offsetMax = Vector2.zero; // Right, Top
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
            }

            SetRaycastTarget(true);
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[{GetType().Name}] OnDrop - Item: {gameObject.name}, Dropped object: {eventData.pointerDrag?.name}");
        }

        private void SetRaycastTarget(bool enabled)
        {
            // Set raycastTarget for main image if assigned
            if (image != null)
                image.raycastTarget = enabled;

            // Set raycastTarget for all child images
            if (childImages != null)
            {
                foreach (var childImage in childImages)
                {
                    if (childImage != null)
                        childImage.raycastTarget = enabled;
                }
            }
        }
    }
}
