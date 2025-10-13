using PawnShop.Controllers.DragNDrop;
using PawnShop.Models.Tags;
using PawnShop.Services;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class CardController : DraggableItemController<BaseTagModel>
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private TextMeshProUGUI _effect;
    [SerializeField] private Outline _outline;
    [SerializeField] private Image _effectPanel;

    private BaseTagModel _model;
    private ILocalizationService _localizationService;

    public BaseTagModel Model => _model;

    [Inject]
    public void Construct(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public override void Init(BaseTagModel tagModel)
    {
        Debug.Log($"[CardController] Init - TagModel: {tagModel.DisplayName}");

        base.Init(tagModel);
        _model = tagModel;

        Debug.Log($"[CardController] Init - Model: {_model.DisplayName}");
        if (_model == null) return;
        if (tagModel == null) return;
        // Set title and description with localization
        _title.text = _localizationService?.GetLocalization(tagModel.DisplayName) ?? tagModel.DisplayName;

        if (_description != null)
        {
            _description.text = _localizationService?.GetLocalization(tagModel.Description) ?? tagModel.Description;
        }

        // Set effect value
        if (_effect != null)
        {
            var effectText = _localizationService?.GetLocalization("card_effect_value") ?? "Effect {0}%";
            var difference = ((tagModel.PriceMultiplier - 1f) * 100).ToString("+0;-0;0");
            _effect.text = string.Format(effectText, difference);
        }

        // Set outline color from tag color
        if (_outline != null)
        {
            _outline.effectColor = tagModel.Color;
        }

        // Set effect panel color from tag color
        if (_effectPanel != null)
        {
            _effectPanel.color = tagModel.Color;
        }

        // Set effect color based on tag value
        if (_effect != null)
        {
            // Green for positive effect, red for negative effect
            _effect.color = tagModel.PriceMultiplier > 1f ? new Color(0.2f, 0.8f, 0.2f) : Color.red;
        }

        // Set tag image using parent's Image component
        if (image != null && tagModel.Image != null)
        {
            image.sprite = tagModel.Image;
            image.preserveAspect = true;
        }
    }
}
