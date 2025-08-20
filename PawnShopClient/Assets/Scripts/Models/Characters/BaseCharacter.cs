using System.Collections.Generic;

public abstract class BaseCharacter : ICharacter
{
    // Skills data only
    public Dictionary<SkillType, Skill> Skills { get; set; } = new();
}
