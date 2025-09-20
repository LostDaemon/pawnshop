using UnityEngine;
using UnityEngine.UI;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public class WorkshopController : MonoBehaviour
    {
        [SerializeField] private Button _repairButton;
        [SerializeField] private Button _cleaningButton;
        [SerializeField] private Button _recycleButton;
        [SerializeField] private Button _polishButton;

        private IWorkshopService _workshopService;

        [Inject]
        public void Construct(IWorkshopService workshopService)
        {
            _workshopService = workshopService;

            // Subscribe to workshop events
            _workshopService.OnItemAdded += OnItemAdded;
            _workshopService.OnItemRemoved += OnItemRemoved;
            _workshopService.OnTaskCompleted += OnTaskCompleted;

            // Setup buttons
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_repairButton != null)
                _repairButton.onClick.AddListener(OnRepairClicked);

            if (_cleaningButton != null)
                _cleaningButton.onClick.AddListener(OnCleaningClicked);

            if (_recycleButton != null)
                _recycleButton.onClick.AddListener(OnRecycleClicked);

            if (_polishButton != null)
                _polishButton.onClick.AddListener(OnPolishClicked);
        }

        private void OnRepairClicked()
        {
            _workshopService.ScheduleTask(ProcessingType.Repair);
        }

        private void OnCleaningClicked()
        {
            _workshopService.ScheduleTask(ProcessingType.Cleaning);
        }

        private void OnRecycleClicked()
        {
            _workshopService.ScheduleTask(ProcessingType.Recycle);
        }

        private void OnPolishClicked()
        {
            _workshopService.ScheduleTask(ProcessingType.Polish);
        }

        private void OnItemAdded(ItemModel item)
        {
            Debug.Log($"[WorkshopController] Item added to workshop: {item.Name}");
            UpdateButtonsState();
        }

        private void OnItemRemoved(ItemModel item)
        {
            Debug.Log($"[WorkshopController] Item removed from workshop: {item.Name}");
            UpdateButtonsState();
        }

        private void OnTaskCompleted(ProcessingTask task)
        {
            Debug.Log($"[WorkshopController] Task completed: {task.ProcessingType} for {task.Item.Name}");
            // Update UI to reflect item changes
            UpdateButtonsState();
        }

        private void UpdateButtonsState()
        {
            var currentItem = _workshopService.CurrentItem;
            bool hasItem = currentItem != null;

            if (_repairButton != null)
                _repairButton.interactable = hasItem && _workshopService.CanPerformTask(currentItem, ProcessingType.Repair);

            if (_cleaningButton != null)
                _cleaningButton.interactable = hasItem && _workshopService.CanPerformTask(currentItem, ProcessingType.Cleaning);

            if (_recycleButton != null)
                _recycleButton.interactable = hasItem; // Recycle is always available when there's an item

            if (_polishButton != null)
                _polishButton.interactable = hasItem && _workshopService.CanPerformTask(currentItem, ProcessingType.Polish);
        }

        private void OnDestroy()
        {
            if (_workshopService != null)
            {
                _workshopService.OnItemAdded -= OnItemAdded;
                _workshopService.OnItemRemoved -= OnItemRemoved;
                _workshopService.OnTaskCompleted -= OnTaskCompleted;
            }

            if (_repairButton != null)
                _repairButton.onClick.RemoveListener(OnRepairClicked);

            if (_cleaningButton != null)
                _cleaningButton.onClick.RemoveListener(OnCleaningClicked);

            if (_recycleButton != null)
                _recycleButton.onClick.RemoveListener(OnRecycleClicked);

            if (_polishButton != null)
                _polishButton.onClick.RemoveListener(OnPolishClicked);
        }
    }
}
