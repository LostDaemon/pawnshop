using System;
using System.Collections.Generic;

namespace PawnShop.Models.Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Skills data only
        public Dictionary<SkillType, Skill> Skills { get; set; } = new();
    }
}
