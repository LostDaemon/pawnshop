using System.Collections.Generic;
using System.Linq;
using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects.Tags;
using UnityEngine;

namespace PawnShop.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly Dictionary<TagType, List<BaseTagPrototype>> _tagsByType = new();
        private readonly List<BaseTagPrototype> _allTags = new();

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
    }
}
