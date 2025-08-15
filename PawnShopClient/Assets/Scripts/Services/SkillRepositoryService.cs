using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillRepositoryService : ISkillRepositoryService
{
    private readonly List<SkillPrototype> _skills;

    public SkillRepositoryService()
    {
        _skills = new List<SkillPrototype>();
    }

    public void Load()
    {
        _skills.Clear();
        _skills.AddRange(Resources.LoadAll<SkillPrototype>(@"ScriptableObjects\Skills").ToList());
        Debug.Log($"Loaded {_skills.Count} skill prototypes.");
    }

    public SkillPrototype GetSkill(SkillType skillType)
    {
        return _skills.FirstOrDefault(s => s.skillType == skillType);
    }

    public IReadOnlyCollection<SkillPrototype> GetAllSkills()
    {
        return _skills.AsReadOnly();
    }

    public IReadOnlyCollection<SkillPrototype> GetSkillsByCategory(string category)
    {
        if (category == "Negotiation")
        {
            return _skills.Where(s => s.skillType.ToString().StartsWith("Negotiation")).ToList().AsReadOnly();
        }
        else if (category == "Inspection")
        {
            return _skills.Where(s => s.skillType.ToString().StartsWith("Inspection")).ToList().AsReadOnly();
        }

        return new List<SkillPrototype>().AsReadOnly();
    }

    public void AddSkill(SkillPrototype skillPrototype)
    {
        if (skillPrototype != null && !_skills.Contains(skillPrototype))
        {
            _skills.Add(skillPrototype);
        }
    }

    public void RemoveSkill(SkillPrototype skillPrototype)
    {
        if (skillPrototype != null)
        {
            _skills.Remove(skillPrototype);
        }
    }
}
