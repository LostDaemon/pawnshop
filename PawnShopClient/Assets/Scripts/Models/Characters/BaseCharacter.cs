using System.Collections.Generic;

public abstract class BaseCharacter : ICharacter
{
    public string Name { get; set; }

    // Skills data only
    public Dictionary<SkillType, Skill> Skills { get; set; } = new();

    protected BaseCharacter(string name)
    {
        Name = name;
    }
}
