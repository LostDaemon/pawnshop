using UnityEngine;
using PawnShop.Models;
using Zenject;

namespace PawnShop.Services
{
    public class DragNDropService<TPayload> : IDragNDropService<TPayload> where TPayload : class
    {
        private TPayload _currentDragItem;
        private StorageType _currentSourceStorage;
        private bool _isDragging;
        
        public bool IsDragging => _isDragging;
        public TPayload CurrentDragItem => _currentDragItem;
        public StorageType CurrentSourceStorage => _currentSourceStorage;
        
        public void StartDrag(TPayload payload, StorageType sourceStorageType)
        {
            Debug.Log($"[DragNDropService] StartDrag payload: {payload} from storage: {sourceStorageType}");
            _currentDragItem = payload;
            _currentSourceStorage = sourceStorageType;
            _isDragging = true;
        }
        
        public void UpdateDrag(TPayload payload)
        {
            if (!_isDragging) return;
            
            Debug.Log($"[DragNDropService] UpdateDrag payload: {payload}");
        }
        
        public void EndDrag(TPayload payload)
        {
            if (!_isDragging) return;
            
            Debug.Log($"[DragNDropService] EndDrag payload: {payload}");
            _currentDragItem = null;
            _currentSourceStorage = StorageType.Undefined;
            _isDragging = false;
        }
        
        public void HandleDrop(TPayload payload, StorageType targetStorageType)
        {
            if (!_isDragging || _currentDragItem == null) return;
            
            Debug.Log($"[DragNDropService] HandleDrop payload: {payload} from {_currentSourceStorage} to {targetStorageType}");
            
            // Here you can add transfer logic between storages
            // For example, using IItemTransferService
            
            _currentDragItem = null;
            _currentSourceStorage = StorageType.Undefined;
            _isDragging = false;
        }
    }
}
