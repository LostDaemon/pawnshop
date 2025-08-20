using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Skill
{
    // Basic information
    public SkillType SkillType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Glyph { get; set; } // Renamed from Icon
    public IReadOnlyList<SkillRequirement> RequiredSkills { get; set; }

    // Runtime state
    public int Level { get; set; } = 0;
    public int MaxLevel { get; set; } = 5;

    // Computed property - skill is learned if level > 0
    public bool IsLearned => Level > 0; // Changed from field to computed property

    public Skill(SkillPrototype prototype)
    {
        // Copy data from prototype
        SkillType = prototype.skillType;
        DisplayName = prototype.displayName;
        Description = prototype.description;
        Glyph = prototype.glyph; // Renamed
        RequiredSkills = prototype.requiredSkills.AsReadOnly();
        MaxLevel = prototype.maxLevel;

        // Initialize runtime state
        Level = 0;
    }
}
