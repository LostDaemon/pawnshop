using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class SkillController : MonoBehaviour
{
    [Header("Skill Configuration")]
    [SerializeField] private PlayerSkills skillType;
    [SerializeField] private Button skillButton;
    [SerializeField] private TMP_Text titleText;
    
    [Header("Visual States")]
    [SerializeField] private Color learnedColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.gray;
    [SerializeField] private Color availableColor = Color.white;
    
    [Inject] private ISkillService _skillService;
    [Inject] private ISpriteService _spriteService;
    
    private Skill _skillData;
    
    [Inject]
    public void Construct(ISkillService skillService, ISpriteService spriteService)
    {
        _skillService = skillService;
        _spriteService = spriteService;
    }
    
    private void Start()
    {
        if (skillButton == null)
        {
            Debug.LogError($"[SkillController] Skill button not assigned for skill {skillType}");
            return;
        }
        
        if (titleText == null)
        {
            Debug.LogError($"[SkillController] Title text not assigned for skill {skillType}");
            return;
        }
        
        // Subscribe to skill events
        _skillService.OnSkillStatusChanged += OnSkillStatusChanged;
        _skillService.OnSkillLearned += OnSkillLearned;
        
        // Set up button click handler
        skillButton.onClick.AddListener(OnSkillButtonClicked);
        
        // Initialize skill data
        _skillData = _skillService.GetSkillInfo(skillType);
        if (_skillData == null)
        {
            Debug.LogError($"[SkillController] No skill data found for {skillType}");
            return;
        }
        
        // Set initial title
        SetSkillTitle();
        
        // Update button state
        UpdateButtonState();
    }
    
    private void OnDestroy()
    {
        if (_skillService != null)
        {
            _skillService.OnSkillStatusChanged -= OnSkillStatusChanged;
            _skillService.OnSkillLearned -= OnSkillLearned;
        }
        
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        }
    }
    
    private void OnSkillStatusChanged(PlayerSkills skill, bool isLearned)
    {
        if (skill == skillType)
        {
            UpdateButtonState();
        }
    }
    
    private void OnSkillLearned(PlayerSkills skill)
    {
        if (skill == skillType)
        {
            UpdateButtonState();
        }
    }
    
    private void OnSkillButtonClicked()
    {
        if (_skillService.CanLearnSkill(skillType))
        {
            bool success = _skillService.LearnSkill(skillType);
            if (success)
            {
                Debug.Log($"[SkillController] Skill {skillType} learned successfully!");
            }
            else
            {
                Debug.LogWarning($"[SkillController] Failed to learn skill {skillType}");
            }
        }
        else
        {
            Debug.Log($"[SkillController] Skill {skillType} cannot be learned yet");
        }
    }
    
    private void UpdateButtonState()
    {
        if (_skillData == null) return;
        
        bool isLearned = _skillService.IsSkillLearned(skillType);
        bool canLearn = _skillService.CanLearnSkill(skillType);
        
        // Update button interactability
        skillButton.interactable = canLearn && !isLearned;
        
        // Update visual appearance
        if (isLearned)
        {
            // Skill is already learned - bright icon, button disabled
            titleText.color = learnedColor;
            skillButton.interactable = false;
        }
        else if (canLearn)
        {
            // Skill can be learned - bright icon, button enabled
            titleText.color = availableColor;
            skillButton.interactable = true;
        }
        else
        {
            // Skill cannot be learned yet - gray icon, button disabled
            titleText.color = unavailableColor;
            skillButton.interactable = false;
        }
    }
    
    private void SetSkillTitle()
    {
        if (_skillData == null || string.IsNullOrEmpty(_skillData.DisplayName)) return;
        
        titleText.text = _skillData.DisplayName;
    }
    
    // Public method to manually refresh the controller (useful for testing)
    [ContextMenu("Refresh Skill State")]
    public void RefreshSkillState()
    {
        if (_skillData != null)
        {
            UpdateButtonState();
        }
    }
}
