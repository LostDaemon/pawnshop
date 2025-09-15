using System.Collections.Generic;
using PawnShop.Helpers;
using PawnShop.Models;
using PawnShop.Models.Characters;
using PawnShop.Models.Tags;
using UnityEngine;
using Zenject;

namespace PawnShop.Services
{
    public class ItemInspectionService : IItemInspectionService
    {
        // Patience reduction constants for different analysis types
        private const float VISUAL_ANALYSIS_PATIENCE_COST = 5f;
        private const float LEGAL_STATUS_PATIENCE_COST = 10f;
        private const float PURITY_ANALYZER_PATIENCE_COST = 15f;
        private const float DEFECTOSCOPE_PATIENCE_COST = 20f;
        private const float HISTORY_RESEARCH_PATIENCE_COST = 25f;
        private const float DOCUMENT_INSPECTION_PATIENCE_COST = 30f;

        private readonly IPlayerService _playerService;
        private readonly ICustomerService _customerService;
        private readonly INegotiationHistoryService _historyService;
        private readonly ILocalizationService _localizationService;

        [Inject]
        public ItemInspectionService(IPlayerService playerService, ICustomerService customerService,
            INegotiationHistoryService historyService, ILocalizationService localizationService)
        {
            _playerService = playerService;
            _customerService = customerService;
            _historyService = historyService;
            _localizationService = localizationService;
        }

        public List<BaseTagModel> InspectByPlayer(ItemModel item, AnalyzeType analyzeType = AnalyzeType.Undefined)
        {
            var player = _playerService.Player;
            var revealedTags = InspectInternal(player, item, analyzeType);

            // Reduce customer patience when player performs inspection
            ReduceCustomerPatience(analyzeType);

            WriteAnalysisToHistory(item, analyzeType, revealedTags);
            return revealedTags;
        }

        public List<BaseTagModel> InspectByCustomer(ItemModel item, AnalyzeType analyzeType = AnalyzeType.Undefined)
        {
            var customer = _customerService.CurrentCustomer;
            return InspectInternal(customer, item, analyzeType);
        }

        private void WriteAnalysisToHistory(ItemModel item, AnalyzeType analyzeType, List<BaseTagModel> newlyRevealedTags)
        {
            string messageKey;
            switch (analyzeType)
            {
                case AnalyzeType.VisualAnalysis:
                    messageKey = "system_visual_analysis_completed";
                    break;
                case AnalyzeType.CheckLegalStatus:
                    messageKey = "system_legal_status_check_completed";
                    break;
                case AnalyzeType.PurityAnalyzer:
                    messageKey = "system_purity_analysis_completed";
                    break;
                case AnalyzeType.Defectoscope:
                    messageKey = "system_defectoscope_analysis_completed";
                    break;
                case AnalyzeType.HistoryResearch:
                    messageKey = "system_history_research_completed";
                    break;
                case AnalyzeType.DocumentInspection:
                    messageKey = "system_document_inspection_completed";
                    break;
                default: throw new System.ArgumentOutOfRangeException(nameof(analyzeType), analyzeType, null);
            }

            string message = string.Format(_localizationService.GetLocalization(messageKey), item.Name, GetTagsList(newlyRevealedTags));

            // If no tags were revealed, use a different message
            if (newlyRevealedTags.Count == 0)
            {
                string noResultsKey = GetNoResultsMessageKey(analyzeType);
                message = string.Format(_localizationService.GetLocalization(noResultsKey), item.Name);
            }

            _historyService.Add(new TextRecord(HistoryRecordSource.System, message));
        }

        private string GetNoResultsMessageKey(AnalyzeType analyzeType)
        {
            switch (analyzeType)
            {
                case AnalyzeType.VisualAnalysis:
                    return "system_visual_analysis_no_results";
                case AnalyzeType.CheckLegalStatus:
                    return "system_legal_status_check_no_results";
                case AnalyzeType.PurityAnalyzer:
                    return "system_purity_analysis_no_results";
                case AnalyzeType.Defectoscope:
                    return "system_defectoscope_analysis_no_results";
                case AnalyzeType.HistoryResearch:
                    return "system_history_research_no_results";
                case AnalyzeType.DocumentInspection:
                    return "system_document_inspection_no_results";
                default: throw new System.ArgumentOutOfRangeException(nameof(analyzeType), analyzeType, null);
            }
        }

        private string GetTagsList(List<BaseTagModel> tags)
        {
            if (tags.Count == 0)
                return "";

            string result = "";
            for (int i = 0; i < tags.Count; i++)
            {
                result += TagTextRenderHelper.RenderTag(tags[i]);
                if (i < tags.Count - 1)
                {
                    result += " ";
                }
            }
            return result;
        }

        private List<BaseTagModel> InspectInternal(ICharacter character, ItemModel item, AnalyzeType analyzeType)
        {
            var revealedTags = new List<BaseTagModel>();

            if (item?.Tags == null)
            {
                return revealedTags;
            }

            bool isPlayer = character is Player;
            bool isCustomer = character is Customer;

            foreach (var tag in item.Tags)
            {
                bool isTagRevealed = false;
                if (isPlayer)
                {
                    isTagRevealed = tag.IsRevealedToPlayer;
                }
                else if (isCustomer)
                {
                    isTagRevealed = tag.IsRevealedToCustomer;
                }

                if (isTagRevealed)
                {
                    //revealedTags.Add(tag);
                    continue;
                }

                // Check if the analysis type matches the tag's required analysis type
                if (analyzeType != AnalyzeType.Undefined && tag.AnalyzeType != analyzeType)
                {
                    continue; // Skip tags that don't match the analysis type
                }

                if (tag.RequiredSkills == null || tag.RequiredSkills.Length == 0)
                {
                    continue;
                }

                foreach (var skillType in tag.RequiredSkills)
                {
                    if (character.Skills.TryGetValue(skillType, out var skill))
                    {
                        int skillLevel = skill.Level;
                        float chance = skillLevel * 20f;

                        // Add 30% bonus when using analysis tools
                        if (analyzeType != AnalyzeType.Undefined)
                        {
                            chance += 30f;
                        }

                        var randomValue = UnityEngine.Random.Range(0f, 1f) * 100f;
                        if (randomValue <= chance)
                        {
                            if (isPlayer)
                            {
                                tag.IsRevealedToPlayer = true;
                            }
                            else if (isCustomer)
                            {
                                tag.IsRevealedToCustomer = true;
                            }

                            revealedTags.Add(tag);
                            break;
                        }
                    }
                }
            }

            return revealedTags; //Only newly revealed tags
        }

        private void ReduceCustomerPatience(AnalyzeType analyzeType)
        {
            var customer = _customerService.CurrentCustomer;
            if (customer == null)
            {
                return; // No customer present, no patience to reduce
            }

            float patienceCost = GetPatienceCost(analyzeType);

            // Only reduce patience if there's actually a cost
            if (patienceCost > 0f)
            {
                // Use CustomerService method to change patience
                _customerService.ChangeCustomerPatience(-patienceCost);

                Debug.Log($"[ItemInspectionService] Customer patience reduced by {patienceCost:F1}");
            }
        }

        private float GetPatienceCost(AnalyzeType analyzeType)
        {
            return analyzeType switch
            {
                AnalyzeType.VisualAnalysis => VISUAL_ANALYSIS_PATIENCE_COST,
                AnalyzeType.CheckLegalStatus => LEGAL_STATUS_PATIENCE_COST,
                AnalyzeType.PurityAnalyzer => PURITY_ANALYZER_PATIENCE_COST,
                AnalyzeType.Defectoscope => DEFECTOSCOPE_PATIENCE_COST,
                AnalyzeType.HistoryResearch => HISTORY_RESEARCH_PATIENCE_COST,
                AnalyzeType.DocumentInspection => DOCUMENT_INSPECTION_PATIENCE_COST,
                AnalyzeType.Undefined => 0f, // No patience cost for undefined analysis type
                _ => 0f // No patience cost for unknown analysis types
            };
        }
    }
}
