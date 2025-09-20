using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Repositories;
using PawnShop.ScriptableObjects.Tags;
using System.Collections.Generic;

namespace PawnShop.Services
{
    public interface ITagService
    {
        void Upgrade(ItemModel item, TagType tagType);
        void Degrade(ItemModel item, TagType tagType);

        // Factory method
        BaseTagModel GetNewTag(BaseTagPrototype prototype);

        // Repository methods
        BaseTagPrototype GetTagPrototypeByClassId(string classId);
        IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes();
        IReadOnlyCollection<BaseTagPrototype> GetTagPrototypesByType(TagType tagType);
        BaseTagPrototype GetDefaultTagPrototype(DefaultTags defaultTag);
    }
}
