using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ItemController : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _nameText;

    [SerializeField] private string _atlasName = "ItemsAtlas"; // Atlas filename without extension

    public void Show(ItemModel item)
    {
        _nameText.text = item.Name;

        var sprites = Resources.LoadAll<Sprite>(_atlasName);
        var sprite = sprites.FirstOrDefault(s => s.name == item.ImageId);
        Debug.Log($"Sprite found: {sprite != null}");
        if (sprite != null)
            _image.sprite = sprite;
        else
            Debug.LogWarning($"Sprite '{item.ImageId}' not found in atlas '{_atlasName}'");
    }
}