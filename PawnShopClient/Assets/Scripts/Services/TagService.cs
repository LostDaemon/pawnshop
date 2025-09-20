using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Repositories;
using PawnShop.ScriptableObjects.Tags;
using System.Linq;
using UnityEngine;

namespace PawnShop.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ITagFactory _tagFactory;

        public TagService(ITagRepository tagRepository, ITagFactory tagFactory)
        {
            _tagRepository = tagRepository;
            _tagFactory = tagFactory;
        }

        public void Upgrade(ItemModel item, TagType tagType)
        {
            if (item == null) return;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return;

            // Get current tag prototype
            var currentPrototype = _tagRepository.GetTagPrototypeByClassId(currentTag.ClassId);
            if (currentPrototype == null || currentPrototype.NextGradePrototype == null) return;

            // Remove current tag
            item.Tags.Remove(currentTag);

            // Add upgraded tag
            var upgradedTag = CreateTagFromPrototype(currentPrototype.NextGradePrototype);
            upgradedTag.IsRevealedToPlayer = true; // Reveal upgraded tag to player
            item.Tags.Add(upgradedTag);
        }

        public void Degrade(ItemModel item, TagType tagType)
        {
            if (item == null) return;

            // Find current tag of the specified type
            var currentTag = item.Tags.FirstOrDefault(tag => tag.TagType == tagType);
            if (currentTag == null) return;

            // Get current tag prototype
            var currentPrototype = _tagRepository.GetTagPrototypeByClassId(currentTag.ClassId);
            if (currentPrototype == null || currentPrototype.PreviousGradePrototype == null) return;

            // Remove current tag
            item.Tags.Remove(currentTag);

            // Add degraded tag
            var degradedTag = CreateTagFromPrototype(currentPrototype.PreviousGradePrototype);
            degradedTag.IsRevealedToPlayer = true; // Reveal degraded tag to player
            item.Tags.Add(degradedTag);
        }

        private BaseTagModel CreateTagFromPrototype(BaseTagPrototype prototype)
        {
            return _tagFactory.Create(prototype);
        }

        // Factory method
        public BaseTagModel GetNewTag(BaseTagPrototype prototype)
        {
            return _tagFactory.Create(prototype);
        }

        // Repository methods
        public BaseTagPrototype GetTagPrototypeByClassId(string classId)
        {
            return _tagRepository.GetTagPrototypeByClassId(classId);
        }

        public System.Collections.Generic.IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes()
        {
            return _tagRepository.GetAllTagPrototypes();
        }

        public System.Collections.Generic.IReadOnlyCollection<BaseTagPrototype> GetTagPrototypesByType(TagType tagType)
        {
            return _tagRepository.GetTagPrototypesByType(tagType);
        }

        public BaseTagPrototype GetDefaultTagPrototype(DefaultTags defaultTag)
        {
            return _tagRepository.GetDefaultTagPrototype(defaultTag);
        }
    }
}
