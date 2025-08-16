using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillService : ISkillService
{
    private readonly Dictionary<SkillType, Skill> _skills = new();
    private readonly ISkillRepositoryService _skillRepository;

    public event Action<SkillType> OnSkillLearned;

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

        // Notify about skills that became available after learning this skill
        NotifySkillsAvailabilityChanged();

        Debug.Log($"[SkillService] Skill {skill} learned!");
        return true;
    }

    /// <summary>
    /// Notify about skills that may have changed availability status
    /// </summary>
    private void NotifySkillsAvailabilityChanged()
    {
        // Check all skills that might have become available
        foreach (var skill in _skills.Keys)
        {
            if (!_skills[skill].IsLearned && CanLearnSkill(skill))
            {
                // This skill just became available
                OnSkillLearned?.Invoke(skill); // Notify that it's now available
            }
        }
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
        // First, notify about all skills becoming unavailable
        foreach (var skillData in _skills.Values)
        {
            if (skillData.IsLearned)
            {
                OnSkillLearned?.Invoke(skillData.SkillType); // Notify that it's now unavailable
            }
        }

        // Then reset all skills
        foreach (var skillData in _skills.Values)
        {
            skillData.IsLearned = false;
        }

        // Finally, notify about skills that are now available (basic skills with no requirements)
        NotifySkillsAvailabilityChanged();

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
        OnSkillLearned?.Invoke(skill); // Notify that it's now unavailable

        // Notify about skills that may have become unavailable after unlearning this skill
        NotifySkillsAvailabilityChangedAfterUnlearn(skill);

        Debug.Log($"[SkillService] Skill {skill} unlearned");
        return true;
    }

    /// <summary>
    /// Notify about skills that may have changed availability status after unlearning a skill
    /// </summary>
    private void NotifySkillsAvailabilityChangedAfterUnlearn(SkillType unlearnedSkill)
    {
        // Check all skills that might have become unavailable
        foreach (var skill in _skills.Keys)
        {
            if (!_skills[skill].IsLearned)
            {
                // Check if this skill depends on the unlearned skill
                var skillData = _skills[skill];
                if (skillData.RequiredSkills.Contains(unlearnedSkill))
                {
                    // This skill depends on the unlearned skill, so it's now unavailable
                    OnSkillLearned?.Invoke(skill); // Notify that it's now unavailable
                }
            }
        }
    }
}
