using System.Collections.Generic;

public class SkillSet
{
    private readonly Dictionary<SkillType, int> _skills = new();

    public int Get(SkillType type) => _skills.TryGetValue(type, out var level) ? level : 0;

    public void Set(SkillType type, int level)
    {
        _skills[type] = level < 0 ? 0 : level > 5 ? 5 : level;
    }

    public IReadOnlyDictionary<SkillType, int> All => _skills;
}