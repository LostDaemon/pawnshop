using PawnShop.Models;
using System.Collections.Generic;
using UnityEngine;

namespace PawnShop.Services
{
    public class ShelfService : IShelfService
    {
        private readonly Dictionary<string, string> _slotToItemMapping = new();
        
        public void RegisterMapping(string slotId, string itemId)
        {
            if (string.IsNullOrEmpty(slotId) || string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[ShelfService] Attempted to register mapping with null or empty IDs");
                return;
            }
            
            _slotToItemMapping[slotId] = itemId;
            Debug.Log($"[ShelfService] Registered mapping: {slotId} -> {itemId}");
        }
        
        public void UnregisterMapping(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                Debug.LogWarning("[ShelfService] Attempted to unregister mapping with null or empty slot ID");
                return;
            }
            
            if (_slotToItemMapping.Remove(slotId))
            {
                Debug.Log($"[ShelfService] Unregistered mapping for slot: {slotId}");
            }
        }
        
        public bool HasMapping(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
                return false;
                
            return _slotToItemMapping.ContainsKey(slotId);
        }
        
        public string GetItemId(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
                return null;
                
            return _slotToItemMapping.TryGetValue(slotId, out var itemId) ? itemId : null;
        }
        
        public void ClearAllMappings()
        {
            _slotToItemMapping.Clear();
            Debug.Log("[ShelfService] Cleared all mappings");
        }
    }
}
