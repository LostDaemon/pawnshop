using UnityEngine;

public abstract class BaseTagModel
{
    public TagType TagType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public SkillType RequiredSkill { get; set; }
    public bool IsRevealed { get; set; }
    public bool Hidden { get; set; }
    public Color Color { get; set; }
    public float PriceMultiplier { get; set; }
    public float AppearanceChance { get; set; }

    protected BaseTagModel(BaseTagPrototype prototype)
    {
        if (prototype != null)
        {
            TagType = prototype.TagType;
            DisplayName = prototype.DisplayName;
            Description = prototype.Description;
            RequiredSkill = prototype.RequiredSkill;
            PriceMultiplier = prototype.PriceMultiplier;
            AppearanceChance = prototype.AppearanceChance;
            IsRevealed = false;
            Hidden = prototype.Hidden;
            Color = prototype.Color;
        }
    }
}
