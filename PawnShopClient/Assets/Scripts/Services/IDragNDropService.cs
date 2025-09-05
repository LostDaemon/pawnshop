using UnityEngine.EventSystems;

namespace PawnShop.Services
{
    public interface IDragNDropService
    {
        void StartDrag(ITransferPoint<object> source, PointerEventData eventData);
        void UpdateDrag(PointerEventData eventData);
        void EndDrag(PointerEventData eventData);
        void HandleDrop(ITransferPoint<object> target, PointerEventData eventData);
        bool IsDragging { get; }
        ITransferPoint<object> CurrentDragSource { get; }
    }
}
