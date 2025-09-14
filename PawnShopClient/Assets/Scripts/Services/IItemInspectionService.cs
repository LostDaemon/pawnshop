using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public interface IItemInspectionService
    {
        public List<BaseTagModel> InspectByPlayer(ItemModel item, AnalyzeType analyzeType = AnalyzeType.Undefined);
        public List<BaseTagModel> InspectByCustomer(ItemModel item, AnalyzeType analyzeType = AnalyzeType.Undefined);
    }
}
