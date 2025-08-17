using UnityEngine;

public abstract class BaseTagPrototype : BasePrototype
{
    public TagType TagType;
    public string DisplayName;
    public string Description;
    public SkillType RequiredSkill;
    public bool Hidden;
    public Color Color = Color.white;
    [Tooltip("Multiplier that affects item price when this tag is present")]
    [Range(0.0f, 10.0f)]
    public float PriceMultiplier = 1.0f;
    [Tooltip("Probability of this tag appearing on an item (0.0 to 1.0)")]
    [Range(0.0f, 1.0f)]
    public float AppearanceChance = 0.5f;
}
