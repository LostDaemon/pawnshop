using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeListItemController : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private IndicatorController _priceIndicator;

    private static Sprite[] _cachedAtlas;

    public void Show(ItemModel item)
    {
        if (_title != null)
            _title.text = item.Name;

        if (_priceIndicator != null)
            _priceIndicator.SetValue(item.RealPrice, animate: false);

        if (_image != null)
        {
            var sprite = LoadSprite(item.ImageId);
            if (sprite != null)
                _image.sprite = sprite;
            else
                Debug.LogWarning($"Sprite '{item.ImageId}' not found in atlas.");
        }
    }

    private Sprite LoadSprite(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (_cachedAtlas == null)
            _cachedAtlas = Resources.LoadAll<Sprite>("Sprites/ItemsAtlas");

        return System.Array.Find(_cachedAtlas, s => s.name == id);
    }
}