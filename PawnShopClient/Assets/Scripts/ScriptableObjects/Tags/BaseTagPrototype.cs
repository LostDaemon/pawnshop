using PawnShop.Models;
using PawnShop.Models.Tags;
using UnityEngine;

namespace PawnShop.ScriptableObjects.Tags
{
    public abstract class BaseTagPrototype : BasePrototype
    {
        [Header("Tag Properties")]
        public TagType TagType;
        public string DisplayName;
        [TextArea(3, 5)]
        public string Description;
        public SkillType[] RequiredSkills;
        [Tooltip("Whether this tag is revealed to the player by default (visible without inspection)")]
        public bool IsRevealedToPlayer = false;
        [Tooltip("Whether this tag is revealed to the customer by default (visible without inspection)")]
        public bool IsRevealedToCustomer = false;
        public bool Hidden = false;
        public float PriceMultiplier = 1f;
        [Range(0f, 1f)]
        public float AppearanceChance = 1f;
        public Color Color = Color.white;
    }
}
