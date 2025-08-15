using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/SkillPrototype", order = 2)]
public class SkillPrototype : BasePrototype
{
    [Header("Skill Information")]
    public PlayerSkills skillType;
    public string displayName;
    [TextArea(3, 5)]
    public string description;
    public string icon;

    [Header("Dependencies")]
    public List<PlayerSkills> requiredSkills = new();
}
