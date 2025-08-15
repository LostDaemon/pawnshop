using System;
using System.Collections.Generic;

public interface ISkillService
{
    // Check if skill is learned
    bool IsSkillLearned(PlayerSkills skill);
    
    // Learn a skill
    bool LearnSkill(PlayerSkills skill);
    
    // Check if skill can be learned (dependencies met)
    bool CanLearnSkill(PlayerSkills skill);
    
    // Get required skills for a specific skill
    IReadOnlyCollection<PlayerSkills> GetRequiredSkills(PlayerSkills skill);
    
    // Get all learnable skills (dependencies met)
    IReadOnlyCollection<PlayerSkills> GetLearnableSkills();
    
    // Get all learned skills
    IReadOnlyCollection<PlayerSkills> GetLearnedSkills();
    
    // Get all skills with their learned status
    IReadOnlyDictionary<PlayerSkills, bool> GetAllSkills();
    
    // Get skill information
    Skill GetSkillInfo(PlayerSkills skill);
    
    // Get all skills with their info
    IReadOnlyCollection<Skill> GetAllSkillInfos();
    
    // Event when skill is learned
    event Action<PlayerSkills> OnSkillLearned;
    
    // Event when skill status changes
    event Action<PlayerSkills, bool> OnSkillStatusChanged;
}
