using PawnShop.Models;
using PawnShop.Models.Tags;
using UnityEngine;

namespace PawnShop.ScriptableObjects.Tags
{
    public abstract class BaseTagPrototype : BasePrototype
    {
        [Header("Tag Properties")]
        public TagType TagType;
        public AnalyzeType AnalyzeType;
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
        [Tooltip("Processing type")]
        public ProcessingType ProcessingType = ProcessingType.Undefined;
        [Tooltip("Processing duration in minutes")]
        public int ProcessingDurationInMinutes = 10;
        [Range(0f, 1f)]
        [Tooltip("Chance of upgrading this tag (0-1)")]
        public float UpgradeChance = 0f;
        [Tooltip("Previous grade prototype")]
        public BaseTagPrototype PreviousGradePrototype;
        [Tooltip("Next grade prototype")]
        public BaseTagPrototype NextGradePrototype;
        
    }
}
