using UnityEngine;
using UnityEngine.EventSystems;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public abstract class TransferPointBase<TPayload> : MonoBehaviour, ITransferPoint<TPayload> where TPayload : class
    {
        private TPayload _payload;
        [SerializeField] private StorageType _storageType;
        
        private IDragNDropService _dragNDropService;
        
        [Inject]
        public void Construct(IDragNDropService dragNDropService)
        {
            _dragNDropService = dragNDropService;
        }
        
        public virtual TPayload GetPayload()
        {
            return _payload;
        }
        
        public virtual void SetPayload(TPayload payload)
        {
            _payload = payload;
        }
        
        public virtual StorageType StorageType => _storageType;
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] BeginDrag on {gameObject.name} with StorageType: {StorageType}");
            _dragNDropService?.StartDrag(this as ITransferPoint<object>, eventData);
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] Drag on {gameObject.name} at position: {eventData.position}");
            _dragNDropService?.UpdateDrag(eventData);
        }
        
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] EndDrag on {gameObject.name} with StorageType: {StorageType}");
            _dragNDropService?.EndDrag(eventData);
        }
        
        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] Drop on {gameObject.name} with StorageType: {StorageType}");
            _dragNDropService?.HandleDrop(this as ITransferPoint<object>, eventData);
        }
    }
}
