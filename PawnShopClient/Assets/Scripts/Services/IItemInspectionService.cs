using System;
using System.Collections.Generic;

public interface IItemInspectionService
{
    public List<BaseTagModel> Inspect(ICharacter character, ItemModel item);
}
