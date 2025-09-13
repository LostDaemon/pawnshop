using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Controllers
{
    public class ItemDisplayController : MonoBehaviour
    {
        private INegotiationService _negotiationService;

        [SerializeField] private Transform _spawnPoint;

        private GameObject _current;

        [Inject]
        public void Construct(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService;
            _negotiationService.OnNegotiationStarted += OnItemChanged;
        }

        private void OnDestroy()
        {
            if (_negotiationService != null)
            {
                _negotiationService.OnNegotiationStarted -= OnItemChanged;
            }
        }

        private void OnItemChanged()
        {
            if (_negotiationService == null)
            {
                Debug.LogError("[ItemDisplayController] NegotiationService is null in OnItemChanged()");
                return;
            }

            var item = _negotiationService.CurrentItem;

            if (_current != null)
                Destroy(_current);

            var prefab = Resources.Load<GameObject>("UI/ItemPrefab");

            Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
            Transform parent = _spawnPoint != null ? _spawnPoint : null;

            _current = Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            _current.transform.localScale = Vector3.one * item.Scale;

            var renderer = _current.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("ItemPrefab must have a SpriteRenderer");
                return;
            }

            if (item.Image != null)
                renderer.sprite = item.Image;
            else
                Debug.LogWarning($"Sprite not found for item: {item.Name}");

            Debug.Log($"Item displayed in scene: {item.Name}");
        }
    }
}