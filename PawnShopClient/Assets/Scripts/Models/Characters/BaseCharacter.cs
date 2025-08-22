using System.Collections.Generic;

namespace PawnShop.Models.Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        // Skills data only
        public Dictionary<SkillType, Skill> Skills { get; set; } = new();
    }
}
