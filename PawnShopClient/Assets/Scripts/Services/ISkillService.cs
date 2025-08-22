using System;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public interface ISkillService
    {
        // Check if skill is learned to specific level
        bool IsSkillLearnedTo(SkillType skillType, int level);

        // Check if skill is learned (to level 1)
        bool IsSkillLearned(SkillType skillType);

        // Learn a skill
        bool LearnSkill(SkillType skillType);

        // Check if skill can be learned (dependencies met)
        bool CanLearnSkill(SkillType skillType);

        // Get required skills for a specific skill
        IReadOnlyCollection<SkillRequirement> GetRequiredSkills(SkillType skillType);

        // Get all learnable skills (dependencies met)
        IReadOnlyCollection<SkillType> GetLearnableSkills();

        // Get all learned skills
        IReadOnlyCollection<SkillType> GetLearnedSkills();

        // Get all skills with their learned status
        IReadOnlyDictionary<SkillType, bool> GetAllSkills();

        // Get skill information
        Skill GetSkillInfo(SkillType skillType);

        // Get all skills with their info
        IReadOnlyCollection<Skill> GetAllSkillInfos();

        // Level-related methods
        bool CanLevelUpSkill(SkillType skillType);
        bool LevelUpSkill(SkillType skillType);
        int GetSkillLevel(SkillType skillType);
        int GetSkillMaxLevel(SkillType skillType);

        // Event when skill level changes
        event Action<SkillType, int> OnSkillLevelChanged;
    }
}
