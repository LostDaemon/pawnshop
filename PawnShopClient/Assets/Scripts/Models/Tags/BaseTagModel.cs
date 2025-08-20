using UnityEngine;

public abstract class BaseTagModel
{
    public string ClassId { get; set; }
    public TagType TagType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public SkillType[] RequiredSkills { get; set; }
    public float PriceMultiplier { get; set; }
    public float AppearanceChance { get; set; }
    public bool IsRevealedToPlayer { get; set; }
    public bool IsRevealedToCustomer { get; set; }
    public bool Hidden { get; set; }
    public Color Color { get; set; }

    protected BaseTagModel(BaseTagPrototype prototype)
    {
        if (prototype != null)
        {
            ClassId = prototype.ClassId;
            TagType = prototype.TagType;
            DisplayName = prototype.DisplayName;
            Description = prototype.Description;
            RequiredSkills = prototype.RequiredSkills;
            PriceMultiplier = prototype.PriceMultiplier;
            AppearanceChance = prototype.AppearanceChance;
            IsRevealedToPlayer = prototype.IsRevealedToPlayer;
            IsRevealedToCustomer = prototype.IsRevealedToPlayer;
            Hidden = prototype.Hidden;
            Color = prototype.Color;
        }
    }
}
