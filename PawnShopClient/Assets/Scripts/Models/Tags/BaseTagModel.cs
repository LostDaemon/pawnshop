using UnityEngine;

public abstract class BaseTagModel
{
    public TagType TagType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public SkillRequirement[] RequiredSkills { get; set; }
    public float PriceMultiplier { get; set; }
    public float AppearanceChance { get; set; }
    public bool IsRevealed { get; set; }
    public bool Hidden { get; set; }
    public Color Color { get; set; }

    protected BaseTagModel(BaseTagPrototype prototype)
    {
        if (prototype != null)
        {
            TagType = prototype.TagType;
            DisplayName = prototype.DisplayName;
            Description = prototype.Description;
            RequiredSkills = prototype.RequiredSkills;
            PriceMultiplier = prototype.PriceMultiplier;
            AppearanceChance = prototype.AppearanceChance;
            IsRevealed = prototype.IsRevealed;
            Hidden = prototype.Hidden;
            Color = prototype.Color;
        }
    }
}
