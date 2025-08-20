using System.Collections.Generic;

public interface ICharacter
{
    public Dictionary<SkillType, Skill> Skills { get; set; }
}
