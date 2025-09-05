using UnityEngine;
using UnityEngine.EventSystems;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public abstract class TransferPointBase<TPayload> : MonoBehaviour, ITransferPoint<TPayload> where TPayload : class
    {
        private TPayload _payload;
        [SerializeField] private StorageType _storageType;
        
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
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] Drag on {gameObject.name} at position: {eventData.position}");
        }
        
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] EndDrag on {gameObject.name} with StorageType: {StorageType}");
        }
        
        public virtual void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[TransferPoint] Drop on {gameObject.name} with StorageType: {StorageType}");
        }
    }
}
