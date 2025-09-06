using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IDragNDropService<TPayload> where TPayload : class
    {
        void StartDrag(TPayload payload, StorageType sourceStorageType);
        void UpdateDrag(TPayload payload);
        void EndDrag(TPayload payload);
        void HandleDrop(TPayload payload, StorageType targetStorageType);
        bool IsDragging { get; }
        TPayload CurrentDragItem { get; }
        StorageType CurrentSourceStorage { get; }
    }
}
