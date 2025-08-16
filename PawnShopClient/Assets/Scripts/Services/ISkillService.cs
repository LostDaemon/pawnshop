using System;
using System.Collections.Generic;

public interface ISkillService
{
    // Check if skill is learned
    bool IsSkillLearned(SkillType skill);

    // Learn a skill
    bool LearnSkill(SkillType skill);

    // Check if skill can be learned (dependencies met)
    bool CanLearnSkill(SkillType skill);

    // Get required skills for a specific skill
    IReadOnlyCollection<SkillType> GetRequiredSkills(SkillType skill);

    // Get all learnable skills (dependencies met)
    IReadOnlyCollection<SkillType> GetLearnableSkills();

    // Get all learned skills
    IReadOnlyCollection<SkillType> GetLearnedSkills();

    // Get all skills with their learned status
    IReadOnlyDictionary<SkillType, bool> GetAllSkills();

    // Get skill information
    Skill GetSkillInfo(SkillType skill);

    // Get all skills with their info
    IReadOnlyCollection<Skill> GetAllSkillInfos();

    // Event when skill is learned
    event Action<SkillType> OnSkillLearned;
}
