using System.Collections.Generic;

public interface IItemInspectionService
{
    public List<BaseTagModel> InspectByPlayer(ItemModel item);
    public List<BaseTagModel> InspectByCustomer(ItemModel item);
}
