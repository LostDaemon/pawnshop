using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillService : ISkillService
{
    private readonly Dictionary<SkillType, Skill> _skills = new();
    private readonly ISkillRepositoryService _skillRepository;

    public event Action<SkillType> OnSkillLearned;
    public event Action<SkillType, bool> OnSkillStatusChanged;

    public SkillService(ISkillRepositoryService skillRepository)
    {
        _skillRepository = skillRepository;
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        // Initialize all skills as not learned
        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            if (skill != SkillType.Undefined)
            {
                var prototype = _skillRepository.GetSkill(skill);
                if (prototype != null)
                {
                    _skills[skill] = new Skill(prototype);
                }
                else
                {
                    Debug.LogWarning($"[SkillService] No prototype found for skill {skill}");
                }
            }
        }

        Debug.Log($"[SkillService] Skills initialized. Loaded {_skills.Count} skills.");
    }

    public bool IsSkillLearned(SkillType skill)
    {
        if (skill == SkillType.Undefined)
            return false;

        return _skills.TryGetValue(skill, out var skillData) && skillData.IsLearned;
    }

    public bool LearnSkill(SkillType skill)
    {
        if (skill == SkillType.Undefined)
            return false;

        if (IsSkillLearned(skill))
        {
            Debug.Log($"[SkillService] Skill {skill} is already learned");
            return false;
        }

        if (!CanLearnSkill(skill))
        {
            Debug.Log($"[SkillService] Cannot learn skill {skill} - dependencies not met");
            return false;
        }

        _skills[skill].IsLearned = true;
        OnSkillLearned?.Invoke(skill);
        OnSkillStatusChanged?.Invoke(skill, true);

        Debug.Log($"[SkillService] Skill {skill} learned!");
        return true;
    }

    public bool CanLearnSkill(SkillType skill)
    {
        if (skill == SkillType.Undefined)
            return false;

        if (IsSkillLearned(skill))
            return false;

        var skillData = _skills.GetValueOrDefault(skill);
        if (skillData == null)
            return false;

        // Check if all required skills are learned
        foreach (var requiredSkill in skillData.RequiredSkills)
        {
            if (!IsSkillLearned(requiredSkill))
                return false;
        }

        return true;
    }

    public IReadOnlyCollection<SkillType> GetRequiredSkills(SkillType skill)
    {
        var skillData = _skills.GetValueOrDefault(skill);
        return skillData?.RequiredSkills ?? new List<SkillType>().AsReadOnly();
    }

    public IReadOnlyCollection<SkillType> GetLearnableSkills()
    {
        var learnableSkills = new List<SkillType>();

        foreach (var skill in _skills.Keys)
        {
            if (CanLearnSkill(skill))
            {
                learnableSkills.Add(skill);
            }
        }

        return learnableSkills.AsReadOnly();
    }

    public IReadOnlyCollection<SkillType> GetLearnedSkills()
    {
        return _skills.Where(kvp => kvp.Value.IsLearned).Select(kvp => kvp.Key).ToList().AsReadOnly();
    }

    public IReadOnlyDictionary<SkillType, bool> GetAllSkills()
    {
        return _skills.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.IsLearned);
    }

    // Get skill information
    public Skill GetSkillInfo(SkillType skill)
    {
        return _skills.GetValueOrDefault(skill);
    }

    // Get all skills with their info
    public IReadOnlyCollection<Skill> GetAllSkillInfos()
    {
        return _skills.Values;
    }

    // Additional utility methods

    /// <summary>
    /// Get count of learned skills
    /// </summary>
    public int GetLearnedSkillsCount()
    {
        return _skills.Values.Count(skill => skill.IsLearned);
    }

    /// <summary>
    /// Reset all skills to not learned
    /// </summary>
    public void ResetAllSkills()
    {
        foreach (var skillData in _skills.Values)
        {
            bool wasLearned = skillData.IsLearned;
            skillData.IsLearned = false;

            if (wasLearned)
            {
                OnSkillStatusChanged?.Invoke(skillData.SkillType, false);
            }
        }
        Debug.Log("[SkillService] All skills reset");
    }

    /// <summary>
    /// Unlearn a specific skill
    /// </summary>
    public bool UnlearnSkill(SkillType skill)
    {
        if (skill == SkillType.Undefined)
            return false;

        if (!IsSkillLearned(skill))
        {
            Debug.Log($"[SkillService] Skill {skill} is not learned");
            return false;
        }

        _skills[skill].IsLearned = false;
        OnSkillStatusChanged?.Invoke(skill, false);

        Debug.Log($"[SkillService] Skill {skill} unlearned");
        return true;
    }
}
