using System.Collections.Generic;

public interface ICharacter
{
    // Basic character information
    public string Name { get; set; }
    public Dictionary<SkillType, Skill> Skills { get; set; }
}
