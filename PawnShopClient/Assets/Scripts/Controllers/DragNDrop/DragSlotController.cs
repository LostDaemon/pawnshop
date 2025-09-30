using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace PawnShop.Controllers.DragNDrop
{
    public abstract class DragSlotController<T> : MonoBehaviour, IDropHandler
    {
        [SerializeField] public bool canReceiveDragged = true;
        
        // Events for drag and drop operations
        public event Action<DraggableItemController<T>, PointerEventData> OnItemDroppedEvent;
        public virtual void OnDrop(PointerEventData eventData)
        {
            if (!canReceiveDragged) return;
            
            Debug.Log($"[{GetType().Name}] OnDrop - Slot: {gameObject.name}, Dropped object: {eventData.pointerDrag?.name}");

            GameObject dropped = eventData.pointerDrag;
            if (dropped != null)
            {
                var draggableItem = dropped.GetComponent<DraggableItemController<T>>();
                if (draggableItem != null)
                {
                    // Set the slot as the new parent for the dropped item
                    draggableItem.parentAfterDrag = transform;
                    Debug.Log($"[{GetType().Name}] Item {dropped.name} dropped into slot {gameObject.name}");

                    // Call custom drop logic
                    OnItemDropped(draggableItem, eventData);
                    
                    // Invoke drop event
                    OnItemDroppedEvent?.Invoke(draggableItem, eventData);
                }
            }
        }

        /// <summary>
        /// Override this method to implement custom drop logic for specific item types
        /// </summary>
        /// <param name="draggableItem">The item that was dropped</param>
        /// <param name="eventData">Event data from the drop</param>
        protected virtual void OnItemDropped(DraggableItemController<T> draggableItem, PointerEventData eventData)
        {
            // Override in derived classes for custom behavior
        }
    }
}
