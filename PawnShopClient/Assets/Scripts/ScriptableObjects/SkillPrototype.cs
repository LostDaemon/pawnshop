using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/SkillPrototype", order = 2)]
public class SkillPrototype : BasePrototype
{
    [Header("Skill Information")]
    public SkillType skillType;
    public string displayName;
    [TextArea(3, 5)]
    public string description;

    [Tooltip("FontAwesome Hex Code (ignore \\u prefix)")]
    public string icon; // Hex code for FontAwesome glyph (e.g., "\uF004")

    [Header("Dependencies")]
    public List<SkillType> requiredSkills = new();
}
