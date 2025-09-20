using System;
using PawnShop.Models;

namespace PawnShop.Services
{
    public interface ILaboratoryService
    {
        ItemModel CurrentItem { get; }
        event Action<ItemModel> OnItemAdded;
        event Action<ItemModel> OnItemRemoved;
        event Action<ProcessingTask> OnTaskCompleted;
        void ScheduleTask(ProcessingType taskType);
        bool CanPerformTask(ItemModel item, ProcessingType taskType);
    }
}
