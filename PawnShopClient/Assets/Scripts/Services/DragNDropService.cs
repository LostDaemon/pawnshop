using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace PawnShop.Services
{
    public class DragNDropService : IDragNDropService
    {
        private ITransferPoint<object> _currentDragSource;
        private bool _isDragging;
        
        public bool IsDragging => _isDragging;
        public ITransferPoint<object> CurrentDragSource => _currentDragSource;
        
        public void StartDrag(ITransferPoint<object> source, PointerEventData eventData)
        {
            Debug.Log($"[DragNDropService] StartDrag from {source.GetType().Name}");
            _currentDragSource = source;
            _isDragging = true;
        }
        
        public void UpdateDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            Debug.Log($"[DragNDropService] UpdateDrag at position: {eventData.position}");
        }
        
        public void EndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            Debug.Log($"[DragNDropService] EndDrag from {_currentDragSource?.GetType().Name}");
            _currentDragSource = null;
            _isDragging = false;
        }
        
        public void HandleDrop(ITransferPoint<object> target, PointerEventData eventData)
        {
            if (!_isDragging || _currentDragSource == null) return;
            
            Debug.Log($"[DragNDropService] HandleDrop from {_currentDragSource.GetType().Name} to {target.GetType().Name}");
            
            // Transfer payload from source to target
            var payload = _currentDragSource.GetPayload();
            if (payload != null)
            {
                target.SetPayload(payload);
                _currentDragSource.SetPayload(null);
            }
            
            _currentDragSource = null;
            _isDragging = false;
        }
    }
}
