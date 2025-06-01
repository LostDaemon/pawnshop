using UnityEngine;

public class ItemLotController : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private static Sprite[] _cachedAtlas;

    public ItemModel Item { get; private set; }
    public Transform Anchor { get; private set; }

    public System.Action<ItemModel> OnClicked;

    public void Initialize(ItemModel item, Transform anchor)
    {
        Item = item;
        Anchor = anchor;

        if (_renderer == null)
            _renderer = GetComponentInChildren<SpriteRenderer>();

        if (_renderer == null)
        {
            Debug.LogError("SpriteRenderer not found on ItemLot prefab.");
            return;
        }

        var sprite = LoadSprite(item.ImageId);
        if (sprite != null)
            _renderer.sprite = sprite;
        else
            Debug.LogWarning($"Sprite '{item.ImageId}' not found in atlas.");

        transform.localScale = Vector3.one * item.Scale;
    }

    private Sprite LoadSprite(string imageId)
    {
        if (_cachedAtlas == null)
            _cachedAtlas = Resources.LoadAll<Sprite>("Sprites/ItemsAtlas");

        return System.Array.Find(_cachedAtlas, s => s.name == imageId);
    }

    private void OnMouseDown()
    {
        OnClicked?.Invoke(Item);
    }
}