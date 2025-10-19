using System;
using System.Collections.Generic;

namespace PawnShop.Models.Characters
{
    public interface ICharacter
    {
        public string Id { get; set; }
        public Dictionary<SkillType, Skill> Skills { get; set; }
    }
}
