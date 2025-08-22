using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IItemProcessingService
    {
        void Process(ItemModel item, ItemProcess itemProcess);
    }
}