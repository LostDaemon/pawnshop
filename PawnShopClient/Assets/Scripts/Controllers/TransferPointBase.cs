using UnityEngine;
using System;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public abstract class TransferPointBase : MonoBehaviour, ITransferPoint
    {
        [SerializeField] private string id = "";
        [SerializeField] private StorageType storageType = StorageType.Undefined;
        [SerializeField] private ItemModel item;
        
        public string Id => id;
        public StorageType StorageType => storageType;
        public ItemModel Item 
        { 
            get => item; 
            set => item = value; 
        }
        public bool IsEmpty => item == null;
        
        protected virtual void OnAwake()
        {
            // Override in derived classes for custom initialization
        }
        
        protected virtual void OnInteraction()
        {
            // Override in derived classes for custom interaction logic
        }
        
        public virtual void Highlight(bool isHighlighted)
        {
            // Override in derived classes for custom highlight logic
        }
        
        private void Awake()
        {
            OnAwake();
        }
        
        private void OnMouseDown()
        {
            OnInteraction();
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            // Generate ID when component is first added (only in editor)
            if (string.IsNullOrEmpty(id))
            {
                id = $"{GetIdPrefix()}_{Guid.NewGuid()}";
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Generated new ID for {gameObject.name}: {id}");
            }
        }

        private void OnValidate()
        {
            // Ensure ID is set (only in editor, only for prefab instances)
            if (string.IsNullOrEmpty(id) && UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this) == UnityEditor.PrefabInstanceStatus.Connected)
            {
                id = $"{GetIdPrefix()}_{Guid.NewGuid()}";
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Generated new ID for {gameObject.name}: {id}");
            }
        }
#endif
        
        protected abstract string GetIdPrefix();
    }
}
