using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.ScriptableObjects;

namespace PawnShop.Repositories
{
    public interface ISkillRepository
    {
        SkillPrototype GetSkill(SkillType skillType);
        IReadOnlyCollection<SkillPrototype> GetAllSkills();
        IReadOnlyCollection<SkillPrototype> GetSkillsByCategory(string category);
        void Load();
        void AddSkill(SkillPrototype skillPrototype);
        void RemoveSkill(SkillPrototype skillPrototype);
    }
}
