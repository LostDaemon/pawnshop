using System;
using System.Collections.Generic;

namespace PawnShop.Models.Characters
{
    public abstract class BaseCharacter : ICharacter
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Skills data only
        public Dictionary<SkillType, Skill> Skills { get; set; } = new();
    }
}
