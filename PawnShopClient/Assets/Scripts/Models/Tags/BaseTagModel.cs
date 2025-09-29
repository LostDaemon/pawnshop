using PawnShop.ScriptableObjects.Tags;
using UnityEngine;

namespace PawnShop.Models.Tags
{
    public abstract class BaseTagModel
    {
        public string ClassId { get; set; }
        public TagType TagType { get; set; }
        public AnalyzeType AnalyzeType { get; set; }
        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public SkillType[] RequiredSkills { get; set; }
        public float PriceMultiplier { get; set; }
        public float AppearanceChance { get; set; }
        public bool IsRevealedToPlayer { get; set; }
        public bool IsRevealedToCustomer { get; set; }
        public bool Hidden { get; set; }
        public Color Color { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public int ProcessingDurationInMinutes { get; set; }
        public float UpgradeChance { get; set; }

        protected BaseTagModel(BaseTagPrototype prototype)
        {
            if (prototype != null)
            {
                ClassId = prototype.ClassId;
                TagType = prototype.TagType;
                AnalyzeType = prototype.AnalyzeType;
                DisplayName = prototype.DisplayName;
                Icon = prototype.Icon;
                Description = prototype.Description;
                RequiredSkills = prototype.RequiredSkills;
                PriceMultiplier = prototype.PriceMultiplier;
                AppearanceChance = prototype.AppearanceChance;
                IsRevealedToPlayer = prototype.IsRevealedToPlayer;
                IsRevealedToCustomer = prototype.IsRevealedToCustomer;
                Hidden = prototype.Hidden;
                Color = prototype.Color;
                ProcessingType = prototype.ProcessingType;
                ProcessingDurationInMinutes = prototype.ProcessingDurationInMinutes;
                UpgradeChance = prototype.UpgradeChance;
            }
        }
    }
}
