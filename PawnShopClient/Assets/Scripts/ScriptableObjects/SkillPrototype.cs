using UnityEngine;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/SkillPrototype", order = 2)]
    public class SkillPrototype : BasePrototype
    {
        [Header("Skill Information")]
        public SkillType skillType;
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public int maxLevel = 5;

        [Tooltip("FontAwesome Hex Code (ignore \\u prefix)")]
        public string glyph; // Hex code for FontAwesome glyph (e.g., "\uF004")

        [Header("Dependencies")]
        public List<SkillRequirement> requiredSkills = new();
    }
}
