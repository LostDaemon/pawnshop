using System.Collections.Generic;

[System.Serializable]
public class Skill
{
    // Data from prototype
    public SkillType SkillType { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public IReadOnlyList<SkillType> RequiredSkills { get; set; }

    // Runtime state
    public bool IsLearned { get; set; }

    public Skill(SkillPrototype prototype)
    {
        // Copy data from prototype
        SkillType = prototype.skillType;
        DisplayName = prototype.displayName;
        Description = prototype.description;
        Icon = prototype.icon;
        RequiredSkills = prototype.requiredSkills.AsReadOnly();

        // Initialize runtime state
        IsLearned = false;
    }

    public Skill(SkillPrototype prototype, bool isLearned)
    {
        // Copy data from prototype
        SkillType = prototype.skillType;
        DisplayName = prototype.displayName;
        Description = prototype.description;
        Icon = prototype.icon;
        RequiredSkills = prototype.requiredSkills.AsReadOnly();

        // Set runtime state
        IsLearned = isLearned;
    }
}
