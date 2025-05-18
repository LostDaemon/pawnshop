using UnityEngine;
using Zenject;

public class ItemDisplayController : MonoBehaviour
{
    [Inject] private INegotiateService _purchaseService;

    [SerializeField] private Transform _spawnPoint;

    private GameObject _current;

    private void Start()
    {
        _purchaseService.OnCurrentItemChanged += OnItemChanged;

        if (_purchaseService.CurrentItem != null)
            OnItemChanged(_purchaseService.CurrentItem);
    }

    private void OnDestroy()
    {
        if (_purchaseService != null)
            _purchaseService.OnCurrentItemChanged -= OnItemChanged;
    }

    private void OnItemChanged(ItemModel item)
    {
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

        var sprites = Resources.LoadAll<Sprite>("Sprites/ItemsAtlas");
        var sprite = System.Array.Find(sprites, s => s.name == item.ImageId);

        if (sprite != null)
            renderer.sprite = sprite;
        else
            Debug.LogWarning($"Sprite not found: {item.ImageId}");

        Debug.Log($"Item displayed in scene: {item.Name}");
    }
}