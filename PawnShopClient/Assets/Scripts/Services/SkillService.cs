using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillService : ISkillService
{
    private readonly ISkillRepositoryService _skillRepository;
    private readonly IPlayerService _playerService;

    public event Action<SkillType, int> OnSkillLevelChanged;

    public Dictionary<SkillType, Skill> PlayerSkills => _playerService.Player.Skills;

    public SkillService(ISkillRepositoryService skillRepository, IPlayerService playerService)
    {
        _skillRepository = skillRepository;
        _playerService = playerService;
    }

    public bool IsSkillLearnedTo(SkillType skillType, int level)
    {
        if (skillType == SkillType.Undefined)
            return false;

        return PlayerSkills.TryGetValue(skillType, out var skill) && skill.Level >= level;
    }

    public bool IsSkillLearned(SkillType skillType)
    {
        return IsSkillLearnedTo(skillType, 1);
    }

    public bool LearnSkill(SkillType skillType)
    {
        if (skillType == SkillType.Undefined)
            return false;

        if (!CanLearnSkill(skillType))
        {
            Debug.Log($"[SkillService] Cannot learn skill {skillType} - dependencies not met");
            return false;
        }

        var skill = PlayerSkills[skillType];
        skill.Level++;
        OnSkillLevelChanged?.Invoke(skillType, skill.Level);

        // Notify about skills that became available after learning this skill
        NotifySkillsAvailabilityChanged();

        Debug.Log($"[SkillService] Skill {skillType} leveled up to {skill.Level}!");
        return true;
    }

    /// <summary>
    /// Notify about skills that may have changed availability status
    /// </summary>
    private void NotifySkillsAvailabilityChanged()
    {
        // Check all skills that might have become available
        foreach (var skill in PlayerSkills.Keys)
        {
            if (PlayerSkills[skill].Level == 0 && CanLearnSkill(skill))
            {
                // This skill just became available
                OnSkillLevelChanged?.Invoke(skill, 0); // Notify that it's now available
            }
        }
    }

    public bool CanLearnSkill(SkillType skillType)
    {
        if (skillType == SkillType.Undefined)
            return false;

        var skill = PlayerSkills.GetValueOrDefault(skillType);
        if (skill == null)
            return false;

        // Check if all required skills are learned to required levels
        foreach (var requiredSkill in skill.RequiredSkills)
        {
            if (!IsSkillLearnedTo(requiredSkill.SkillType, requiredSkill.RequiredLevel))
                return false;
        }

        // Can learn if not at max level
        return skill.Level < skill.MaxLevel;
    }

    public bool CanLevelUpSkill(SkillType skillType)
    {
        return CanLearnSkill(skillType);
    }

    public bool LevelUpSkill(SkillType skillType)
    {
        return LearnSkill(skillType);
    }

    public int GetSkillLevel(SkillType skillType)
    {
        return PlayerSkills.TryGetValue(skillType, out var skill) ? skill.Level : 0;
    }

    public int GetSkillMaxLevel(SkillType skillType)
    {
        if (!PlayerSkills.TryGetValue(skillType, out var skill))
            return 0;

        var prototype = _skillRepository.GetSkill(skillType);
        return prototype?.maxLevel ?? 0;
    }

    public IReadOnlyCollection<SkillRequirement> GetRequiredSkills(SkillType skillType)
    {
        var skill = PlayerSkills.GetValueOrDefault(skillType);
        return skill?.RequiredSkills ?? new List<SkillRequirement>().AsReadOnly();
    }

    public IReadOnlyCollection<SkillType> GetLearnableSkills()
    {
        var learnableSkills = new List<SkillType>();

        foreach (var skill in PlayerSkills.Keys)
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
        return PlayerSkills.Where(kvp => kvp.Value.Level > 0).Select(kvp => kvp.Key).ToList().AsReadOnly();
    }

    public IReadOnlyDictionary<SkillType, bool> GetAllSkills()
    {
        return PlayerSkills.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Level > 0);
    }

    // Get skill information
    public Skill GetSkillInfo(SkillType skillType)
    {
        return PlayerSkills.GetValueOrDefault(skillType);
    }

    // Get all skills with their info
    public IReadOnlyCollection<Skill> GetAllSkillInfos()
    {
        return PlayerSkills.Values;
    }

    // Additional utility methods

    /// <summary>
    /// Get count of learned skills
    /// </summary>
    public int GetLearnedSkillsCount()
    {
        return PlayerSkills.Values.Count(skill => skill.Level > 0);
    }

    /// <summary>
    /// Reset all skills to not learned
    /// </summary>
    public void ResetAllSkills()
    {
        // First, notify about all skills becoming unavailable
        foreach (var skill in PlayerSkills.Values)
        {
            if (skill.Level > 0)
            {
                OnSkillLevelChanged?.Invoke(skill.SkillType, 0); // Notify that it's now unavailable
            }
        }

        // Then reset all skills
        foreach (var skill in PlayerSkills.Values)
        {
            skill.Level = 0;
        }

        // Finally, notify about skills that are now available (basic skills with no requirements)
        NotifySkillsAvailabilityChanged();

        Debug.Log("[SkillService] All skills reset");
    }

    /// <summary>
    /// Unlearn a specific skill
    /// </summary>
    public bool UnlearnSkill(SkillType skillType)
    {
        if (skillType == SkillType.Undefined)
            return false;

        if (!IsSkillLearned(skillType))
        {
            Debug.Log($"[SkillService] Skill {skillType} is not learned");
            return false;
        }

        PlayerSkills[skillType].Level = 0;
        OnSkillLevelChanged?.Invoke(skillType, 0); // Notify that it's now unavailable

        // Notify about skills that may have become unavailable after unlearning this skill
        NotifySkillsAvailabilityChangedAfterUnlearn(skillType);

        Debug.Log($"[SkillService] Skill {skillType} unlearned");
        return true;
    }

    /// <summary>
    /// Notify about skills that may have changed availability status after unlearning a skill
    /// </summary>
    private void NotifySkillsAvailabilityChangedAfterUnlearn(SkillType unlearnedSkill)
    {
        // Check all skills that might have become unavailable
        foreach (var skill in PlayerSkills.Keys)
        {
            var skillData = PlayerSkills[skill];
            // Check if this skill depends on the unlearned skill
            if (skillData.RequiredSkills.Any(req => req.SkillType == unlearnedSkill))
            {
                // This skill depends on the unlearned skill, so it's now unavailable
                OnSkillLevelChanged?.Invoke(skill, 0); // Notify that it's now unavailable
            }
        }
    }
}
