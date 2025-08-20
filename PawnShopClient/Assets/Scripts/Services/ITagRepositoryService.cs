using System.Collections.Generic;

public interface ITagRepositoryService
{
    void Load();
    BaseTagPrototype GetTagPrototypeByClassId(string classId);
    IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes();
    IReadOnlyCollection<BaseTagPrototype> GetTagPrototypesByType(TagType tagType);
}
