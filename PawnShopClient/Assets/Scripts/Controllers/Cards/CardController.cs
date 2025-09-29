using PawnShop.Controllers.DragNDrop;
using PawnShop.Models.Tags;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardController : DraggableItemController
{
    [SerializeField] private Text _title;
    [SerializeField] private Text _description;
    [SerializeField] private Text _effect;
    [SerializeField] private Outline _outline;

    public void Init(BaseTagModel tagModel)
    {
        if (tagModel == null) return;
        
        // Set title and description
        _title.text = tagModel.DisplayName;
        _description.text = tagModel.Description;
        
        // Set outline color from tag color
        if (_outline != null)
        {
            _outline.effectColor = tagModel.Color;
        }
        
        // Set effect color based on tag value
        if (_effect != null)
        {
            if (tagModel is NumericTagModel numericTag)
            {
                // Green for positive effect, red for negative effect
                _effect.color = numericTag.PriceMultiplier > 1f ? Color.green : Color.red;
            }
            else
            {
                _effect.color = Color.white; // Default color for non-numeric tags
            }
        }
        
        // Set tag icon using existing Image component
        if (!string.IsNullOrEmpty(tagModel.Icon))
        {
            var imageComponent = GetComponent<Image>();
            if (imageComponent != null)
            {
                // TODO: Load icon sprite from resources or asset bundle and set to imageComponent.sprite
            }
        }
    }
}
