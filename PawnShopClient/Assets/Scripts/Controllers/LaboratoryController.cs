using UnityEngine;
using UnityEngine.UI;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Controllers
{
    public class LaboratoryController : MonoBehaviour
    {
        [SerializeField] private Button _researchButton;
        [SerializeField] private Button _forgeDocumentsButton;
        [SerializeField] private Button _evaluateButton;
        [SerializeField] private Button _cleanLegalStatusButton;
        [SerializeField] private ItemSlotController _itemSlotController; 

        private ILaboratoryService _laboratoryService;

        [Inject]
        public void Construct(ILaboratoryService laboratoryService)
        {
            _laboratoryService = laboratoryService;

            // Subscribe to laboratory events
            _laboratoryService.OnItemAdded += OnItemAdded;
            _laboratoryService.OnItemRemoved += OnItemRemoved;
            _laboratoryService.OnTaskCompleted += OnTaskCompleted;

            // Setup buttons
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Initially disable research button
            if (_researchButton != null)
            {
                _researchButton.interactable = false;
                _researchButton.onClick.AddListener(OnResearchClicked);
            }

            // Initially disable forge documents button
            if (_forgeDocumentsButton != null)
            {
                _forgeDocumentsButton.interactable = false;
                _forgeDocumentsButton.onClick.AddListener(OnForgeDocumentsClicked);
            }

            // Initially disable evaluate button
            if (_evaluateButton != null)
            {
                _evaluateButton.interactable = false;
                _evaluateButton.onClick.AddListener(OnEvaluateClicked);
            }

            // Initially disable clean legal status button
            if (_cleanLegalStatusButton != null)
            {
                _cleanLegalStatusButton.interactable = false;
                _cleanLegalStatusButton.onClick.AddListener(OnCleanLegalStatusClicked);
            }
        }

        private void OnResearchClicked()
        {
            _laboratoryService.ScheduleTask(ProcessingType.Research);
            ShowProcess(true);
        }

        private void OnForgeDocumentsClicked()
        {
            _laboratoryService.ScheduleTask(ProcessingType.ForgeDocuments);
            ShowProcess(true);
        }

        private void OnEvaluateClicked()
        {
            _laboratoryService.ScheduleTask(ProcessingType.Evaluate);
            ShowProcess(true);
        }

        private void OnCleanLegalStatusClicked()
        {
            _laboratoryService.ScheduleTask(ProcessingType.CleanLegalStatus);
            ShowProcess(true);
        }

        private void OnItemAdded(ItemModel item)
        {
            Debug.Log($"[LaboratoryController] Item added to laboratory: {item.Name}");
            UpdateButtonsState();
        }

        private void OnItemRemoved(ItemModel item)
        {
            Debug.Log($"[LaboratoryController] Item removed from laboratory: {item.Name}");
            UpdateButtonsState();
        }

        private void OnTaskCompleted(ProcessingTask task)
        {
            Debug.Log($"[LaboratoryController] Task completed: {task.ProcessingType} for {task.Item.Name}");
            
            // Deactivate process icon when task is completed
            ShowProcess(false);
            
            // Find and update ItemController in ItemSlotController children
            if (_itemSlotController != null)
            {
                var itemController = _itemSlotController.GetComponentInChildren<ItemController>();
                if (itemController != null)
                {
                    itemController.UpdateVisualLayers(task.Item);
                }
            }
            
            // Update UI to reflect item changes
            UpdateButtonsState();
        }

        private void ShowProcess(bool isActive)
        {
            if (_itemSlotController != null)
            {
                var itemController = _itemSlotController.GetComponentInChildren<ItemController>();
                if (itemController != null)
                {
                    itemController.ShowProcess(isActive);
                }
            }
        }

        private void UpdateButtonsState()
        {
            var currentItem = _laboratoryService.CurrentItem;
            bool hasItem = currentItem != null;

            if (_researchButton != null)
                _researchButton.interactable = hasItem && _laboratoryService.CanPerformTask(currentItem, ProcessingType.Research);

            if (_forgeDocumentsButton != null)
                _forgeDocumentsButton.interactable = hasItem && _laboratoryService.CanPerformTask(currentItem, ProcessingType.ForgeDocuments);

            if (_evaluateButton != null)
                _evaluateButton.interactable = hasItem && _laboratoryService.CanPerformTask(currentItem, ProcessingType.Evaluate);

            if (_cleanLegalStatusButton != null)
                _cleanLegalStatusButton.interactable = hasItem && _laboratoryService.CanPerformTask(currentItem, ProcessingType.CleanLegalStatus);
        }

        private void OnDestroy()
        {
            if (_laboratoryService != null)
            {
                _laboratoryService.OnItemAdded -= OnItemAdded;
                _laboratoryService.OnItemRemoved -= OnItemRemoved;
                _laboratoryService.OnTaskCompleted -= OnTaskCompleted;
            }

            if (_researchButton != null)
                _researchButton.onClick.RemoveListener(OnResearchClicked);

            if (_forgeDocumentsButton != null)
                _forgeDocumentsButton.onClick.RemoveListener(OnForgeDocumentsClicked);

            if (_evaluateButton != null)
                _evaluateButton.onClick.RemoveListener(OnEvaluateClicked);

            if (_cleanLegalStatusButton != null)
                _cleanLegalStatusButton.onClick.RemoveListener(OnCleanLegalStatusClicked);
        }
    }
}
