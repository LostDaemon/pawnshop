using System.Collections.Generic;
using System.Linq;
using PawnShop.Models;
using PawnShop.Models.Tags;
using PawnShop.Repositories;
using PawnShop.Services;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class LaboratoryService : BaseProcessingService, ILaboratoryService
    {
        private readonly IEvaluationService _evaluationService;

        [Inject]
        public LaboratoryService(IStorageLocatorService storageLocatorService, ITimeService timeService, ITagService tagService, IEvaluationService evaluationService)
            : base(storageLocatorService, timeService, tagService, StorageType.LaboratoryStorage)
        {
            _evaluationService = evaluationService;
        }

        public override bool CanPerformTask(ItemModel item, ProcessingType taskType)
        {
            if (item == null) return false;

            // All laboratory operations are always available when there's an item
            return true;
        }

        protected override void ProcessItemByType(ItemModel item, ProcessingType processingType)
        {
            switch (processingType)
            {
                case ProcessingType.Research:
                    ProcessResearch(item);
                    break;
                case ProcessingType.ForgeDocuments:
                    ProcessForgeDocuments(item);
                    break;
                case ProcessingType.Evaluate:
                    ProcessEvaluate(item);
                    break;
                case ProcessingType.CleanLegalStatus:
                    ProcessCleanLegalStatus(item);
                    break;
                default:
                    ProcessDefaultTask(item, processingType);
                    break;
            }
        }

        private void ProcessResearch(ItemModel item)
        {
            if (item == null) return;

            // Make all hidden tags visible to player
            var hiddenTags = item.Tags.Where(tag => !tag.IsRevealedToPlayer).ToList();

            foreach (var tag in hiddenTags)
            {
                tag.IsRevealedToPlayer = true;
            }

            Debug.Log($"[LaboratoryService] Research completed for {item.Name} - revealed {hiddenTags.Count} hidden tags");
        }

        private void ProcessForgeDocuments(ItemModel item)
        {
            if (item == null) return;

            Debug.Log($"[LaboratoryService] Starting document forgery for {item.Name}");

            // Try to upgrade Documents tag
            if (TryUpgradeTag(item, TagType.Documents))
            {
                Debug.Log($"[LaboratoryService] Document forgery successful - upgraded Documents tag");
            }
            else
            {
                Debug.Log($"[LaboratoryService] Document forgery failed - could not upgrade Documents tag");
            }
        }

        private void ProcessEvaluate(ItemModel item)
        {
            if (item == null) return;

            Debug.Log($"[LaboratoryService] Starting evaluation of {item.Name}");

            // Use EvaluationService to calculate estimated value based on known tags
            long estimatedValue = _evaluationService.EvaluateByPlayer(item, EvaluationStrategy.Realistic);

            Debug.Log($"[LaboratoryService] Evaluation completed for {item.Name} - estimated value: {estimatedValue}");
        }

        protected override GameTime CalculateReadyTime(ProcessingType taskType)
        {
            var currentTime = _timeService.CurrentTime;
            
            // CleanLegalStatus takes exactly 1 game hour (60 minutes)
            if (taskType == ProcessingType.CleanLegalStatus)
            {
                int taskDurationMinutes = 60;
                return currentTime + taskDurationMinutes;
            }
            
            // For other tasks, use default calculation from base class
            return base.CalculateReadyTime(taskType);
        }

        private void ProcessCleanLegalStatus(ItemModel item)
        {
            if (item == null) return;

            Debug.Log($"[LaboratoryService] Starting legal status cleaning for {item.Name}");

            // Find current legal status tag
            var currentLegalStatusTag = item.Tags.FirstOrDefault(tag => tag.TagType == TagType.LegalStatus);
            
            if (currentLegalStatusTag != null)
            {
                // Remove current legal status tag
                item.Tags.Remove(currentLegalStatusTag);
                Debug.Log($"[LaboratoryService] Removed current legal status: {currentLegalStatusTag.DisplayName}");
            }

            // Add Clean legal status tag
            var cleanTagPrototype = _tagService.GetDefaultTagPrototype(DefaultTags.LegalStatusClean);
            if (cleanTagPrototype != null)
            {
                var cleanTag = _tagService.GetNewTag(cleanTagPrototype);
                item.Tags.Add(cleanTag);
                Debug.Log($"[LaboratoryService] Legal status cleaned - assigned Clean status");
            }
            else
            {
                Debug.LogWarning($"[LaboratoryService] Could not find Clean legal status tag prototype");
            }
        }

        protected override string GetServiceName()
        {
            return "LaboratoryService";
        }

        protected override string GetStorageName()
        {
            return "laboratory";
        }
    }
}
