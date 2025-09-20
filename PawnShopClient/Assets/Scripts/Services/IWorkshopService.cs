using System;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface IWorkshopService
    {
        ItemModel CurrentItem { get; }
        event Action<ItemModel> OnItemAdded;
        event Action<ItemModel> OnItemRemoved;
        event Action<ProcessingTask> OnTaskCompleted;
        void ScheduleTask(ProcessingType taskType);
        bool IsDestroyed(ItemModel item);
        bool CanPerformTask(ItemModel item, ProcessingType taskType);
    }
}
