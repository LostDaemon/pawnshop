using System.Collections.Generic;

public interface ITagRepositoryService
{
    void Load();
    BaseTagPrototype GetTagPrototype(TagType tagType);
    IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes();
}
