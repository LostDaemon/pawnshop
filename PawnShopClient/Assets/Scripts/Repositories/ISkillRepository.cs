using System.Collections.Generic;

public interface ISkillRepository
{
    SkillPrototype GetSkill(SkillType skillType);
    IReadOnlyCollection<SkillPrototype> GetAllSkills();
    IReadOnlyCollection<SkillPrototype> GetSkillsByCategory(string category);
    void Load();
    void AddSkill(SkillPrototype skillPrototype);
    void RemoveSkill(SkillPrototype skillPrototype);
}
