using UnityEngine;
using UnityEngine.EventSystems;

namespace PawnShop.Controllers.DragNDrop
{
    public abstract class DragSlotController : MonoBehaviour, IDropHandler
    {
        [SerializeField] public bool canReceiveDragged = true;
        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!canReceiveDragged) return;
            
            Debug.Log($"[{GetType().Name}] OnDrop - Slot: {gameObject.name}, Dropped object: {eventData.pointerDrag?.name}");

            GameObject dropped = eventData.pointerDrag;
            if (dropped != null)
            {
                DraggableItemController draggableItem = dropped.GetComponent<DraggableItemController>();
                if (draggableItem != null)
                {
                    // Set the slot as the new parent for the dropped item
                    draggableItem.parentAfterDrag = transform;
                    Debug.Log($"[{GetType().Name}] Item {dropped.name} dropped into slot {gameObject.name}");

                    // Call custom drop logic
                    OnItemDropped(draggableItem, eventData);
                }
            }
        }

        /// <summary>
        /// Override this method to implement custom drop logic for specific item types
        /// </summary>
        /// <param name="draggableItem">The item that was dropped</param>
        /// <param name="eventData">Event data from the drop</param>
        protected virtual void OnItemDropped(DraggableItemController draggableItem, PointerEventData eventData)
        {
            // Override in derived classes for custom behavior
        }
    }
}
