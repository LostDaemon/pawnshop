using System.Collections.Generic;
using System.Linq;
using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects.Tags;
using UnityEngine;

namespace PawnShop.Repositories
{

    public enum DefaultTags
    {
        Undefined = 0,
        ConditionDestroyed,
        ConditionDamaged,
        ConditionWorn,
        ConditionPristine,
        FeatureLightScratch,
        FeatureDeepScratch,
        FeatureDirt,
        LegalStatusIllegal,
        LegalStatusSuspicious,
        LegalStatusClean,
    }


    public class TagRepository : ITagRepository
    {
        private readonly Dictionary<TagType, List<BaseTagPrototype>> _tagsByType = new();
        private readonly List<BaseTagPrototype> _allTags = new();
        private readonly Dictionary<DefaultTags, string> _defaultTagToClassId = new()
        {
            { DefaultTags.ConditionDestroyed, "5bb58300-dd6f-458b-b2a5-a2c0d045b3cd" },
            { DefaultTags.ConditionDamaged, "5c6f93b5-6717-47c5-acdf-3a12239c1984" },
            { DefaultTags.ConditionWorn, "1eca794e-fc4b-4365-8157-70b694be5dca" },
            { DefaultTags.ConditionPristine, "5aa04753-b152-45d4-8e63-e36c185fced2" },
            { DefaultTags.FeatureLightScratch, "f62a20bd-eb5a-44c3-8431-a91e7fd69739" },
            { DefaultTags.FeatureDeepScratch, "8f9d43a1-1a47-4dab-80f2-f9acb34fd14b" },
            { DefaultTags.FeatureDirt, "b7aef9e4-0210-46de-a99d-9c7c20ed17a9" },
            { DefaultTags.LegalStatusIllegal, "6725ee66-882a-4806-a615-fbc8a2ae7f2d" },
            { DefaultTags.LegalStatusSuspicious, "9b1b86cd-a231-48f6-b59a-72cbb5544d6e" },
            { DefaultTags.LegalStatusClean, "4ef5df09-b104-4fe0-9351-47b2c471129d" }
        };

        public void Load()
        {
            _tagsByType.Clear();
            _allTags.Clear();

            // Load all tag prototypes from Resources
            var tagPrototypes = Resources.LoadAll<BaseTagPrototype>("ScriptableObjects/Tags");

            foreach (var tagPrototype in tagPrototypes)
            {
                if (tagPrototype == null) continue;

                // Add to all tags list
                _allTags.Add(tagPrototype);

                // Group by tag type
                if (!_tagsByType.ContainsKey(tagPrototype.TagType))
                {
                    _tagsByType[tagPrototype.TagType] = new List<BaseTagPrototype>();
                }
                _tagsByType[tagPrototype.TagType].Add(tagPrototype);
            }

            Debug.Log($"[TagRepository] Loaded {_allTags.Count} tag prototypes across {_tagsByType.Count} tag types");
        }

        public BaseTagPrototype GetTagPrototypeByClassId(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return null;

            return _allTags.FirstOrDefault(tag => tag.ClassId == classId);
        }

        public IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes()
        {
            return _allTags.AsReadOnly();
        }

        public IReadOnlyCollection<BaseTagPrototype> GetTagPrototypesByType(TagType tagType)
        {
            if (_tagsByType.TryGetValue(tagType, out var tags))
            {
                return tags.AsReadOnly();
            }
            return new List<BaseTagPrototype>().AsReadOnly();
        }

        public BaseTagPrototype GetDefaultTagPrototype(DefaultTags defaultTag)
        {
            if (!_defaultTagToClassId.TryGetValue(defaultTag, out var classId)) return null;
            return GetTagPrototypeByClassId(classId);
        }
    }
}
