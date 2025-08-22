using System.Collections.Generic;

namespace PawnShop.Models.Characters
{
    public interface ICharacter
    {
        public Dictionary<SkillType, Skill> Skills { get; set; }
    }
}
