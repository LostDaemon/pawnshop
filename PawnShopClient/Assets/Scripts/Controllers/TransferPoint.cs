using UnityEngine;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public class TransferPoint : TransferPointBase
    {
        [SerializeField] private TransferPoint[] _transferTargets;
        
        public TransferPoint[] TransferTargets => _transferTargets;
        private bool _waitingForTarget = false;
        private IItemTransferService _itemTransferService;
        
        [Inject]
        public void Construct(IItemTransferService itemTransferService)
        {
            _itemTransferService = itemTransferService;
        }
        
        protected override void OnInteraction()
        {
            // Set waiting flag and highlight all transfer targets when source is clicked
            SetWaitingState(true);
        }
        
        private void SetWaitingState(bool waiting)
        {
            _waitingForTarget = waiting;
            
            if (_transferTargets != null)
            {
                foreach (var target in _transferTargets)
                {
                    if (target != null)
                    {
                        target.Highlight(waiting);
                    }
                }
            }
        }
        
        private void Update()
        {
            if (_waitingForTarget && Input.GetMouseButtonDown(0))
            {
                HandleMouseTransferClick();
            }
        }
        
        public void HandleTransferClick()
        {
            // For UI button click - transfer to first available target or show selection
            if (_transferTargets != null && _transferTargets.Length > 0)
            {
                TransferPoint firstTarget = _transferTargets[0];
                if (firstTarget != null)
                {
                    TransferItemToTarget(firstTarget);
                }
            }
            else
            {
                Debug.LogWarning("No transfer targets available!");
            }
        }
        
        public void HandleMouseTransferClick()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            TransferPoint clickedTarget = GetClickedTarget(mousePosition);
            if (clickedTarget != null)
            {
                TransferItemToTarget(clickedTarget);
            }
            else
            {
                SetWaitingState(false);
            }
        }
        
        private TransferPoint GetClickedTarget(Vector2 mousePosition)
        {
            if (_transferTargets == null) return null;
            
            foreach (var target in _transferTargets)
            {
                if (target != null)
                {
                    var collider = target.GetComponent<Collider2D>();
                    if (collider != null && collider.OverlapPoint(mousePosition))
                    {
                        return target;
                    }
                }
            }
            return null;
        }
        
        private void TransferItemToTarget(TransferPoint target)
        {
            if (_itemTransferService == null)
            {
                Debug.LogError("ItemTransferService is not injected!");
                return;
            }
            
            if (Item == null)
            {
                Debug.LogWarning("No item to transfer!");
                return;
            }
            
            // Check if transfer is possible
            if (_itemTransferService.CanTransferItem(Item, StorageType, target.StorageType))
            {
                // Perform transfer
                if (_itemTransferService.TransferItem(Item, StorageType, target.StorageType))
                {
                    Debug.Log($"Successfully transferred item {Item.Name} from {StorageType} to {target.StorageType}");
                    // Clear item from source after successful transfer
                    Item = null;
                }
                else
                {
                    Debug.LogError($"Failed to transfer item {Item.Name} from {StorageType} to {target.StorageType}");
                }
            }
            else
            {
                Debug.LogWarning($"Cannot transfer item {Item.Name} from {StorageType} to {target.StorageType}");
            }
            
            // Reset waiting state after transfer attempt
            SetWaitingState(false);
        }
        
        private void ResetWaitingState()
        {
            SetWaitingState(false);
        }
        
        protected override string GetIdPrefix() => "transfer_point";
    }
}
