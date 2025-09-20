using System.Collections.Generic;
using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects.Tags;

namespace PawnShop.Repositories
{
    public interface ITagRepository
    {
        void Load();
        BaseTagPrototype GetTagPrototypeByClassId(string classId);
        IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes();
        IReadOnlyCollection<BaseTagPrototype> GetTagPrototypesByType(TagType tagType);
        BaseTagPrototype GetDefaultTagPrototype(DefaultTags defaultTag);
    }
}
