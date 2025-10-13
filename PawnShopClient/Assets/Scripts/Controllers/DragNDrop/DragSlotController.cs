using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace PawnShop.Controllers.DragNDrop
{
    public abstract class DragSlotController<T> : MonoBehaviour, IDropHandler
    {
        [SerializeField] public bool canReceiveDragged = true;

        // Events for drag and drop operations
        public event Action<DraggableItemController<T>> OnItemStartDragEvent;
        public event Action<DraggableItemController<T>> OnItemDroppedEvent;
        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[DragSlotController] OnDrop: {canReceiveDragged}");
            if (!canReceiveDragged) return;

            GameObject dropped = eventData.pointerDrag;
            if (dropped != null)
            {

                var draggableItem = dropped.GetComponent<DraggableItemController<T>>();
                if (draggableItem != null)
                {
                    Debug.Log($"[DragSlotController] OnDrop!: {draggableItem != null}");
                    // Set the slot as the new parent for the dropped item
                    draggableItem.parentAfterDrag = transform;
                    // Call custom drop logic
                    OnItemDropped(draggableItem, eventData);
                    // Invoke drop event
                    Debug.Log($"[DragSlotController] Invoking OnItemDroppedEvent for: {draggableItem?.name}");
                    OnItemDroppedEvent?.Invoke(draggableItem);
                    Debug.Log($"[DragSlotController] OnItemDroppedEvent invoked");
                }
            }
        }

        /// <summary>
        /// Public method to handle drag out operation
        /// </summary>
        /// <param name="draggableItem">The item being dragged out</param>
        public virtual void DragOut(DraggableItemController<T> draggableItem)
        {
            OnItemStartDragEvent?.Invoke(draggableItem);
        }

        /// <summary>
        /// Override this method to implement custom drop logic for specific item types
        /// </summary>
        /// <param name="draggableItem">The item that was dropped</param>
        /// <param name="eventData">Event data from the drop</param>
        protected virtual void OnItemDropped(DraggableItemController<T> draggableItem, PointerEventData eventData)
        {
        }
    }
}
