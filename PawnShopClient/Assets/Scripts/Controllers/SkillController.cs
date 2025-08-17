using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using System.Linq; // Added for .Where() and .ToList()

public class SkillController : MonoBehaviour
{
    [Header("Skill Configuration")]
    [SerializeField] private SkillType _skillType;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _glyph;

    [Header("Visual States")]
    [SerializeField] private Color _learnedColor = Color.white;
    [SerializeField] private Color _unavailableColor = Color.gray;
    [SerializeField] private Color _availableColor = Color.white;

    [Header("Level Display")]
    [SerializeField] private TMP_Text _levelText;

    [Inject] private ISkillService _skillService;
    private Skill _skillData;
    private Button _skillButton;

    [Inject]
    public void Construct(ISkillService skillService)
    {
        _skillService = skillService;
    }

    private void Start()
    {
        // Get button component from this GameObject
        _skillButton = GetComponent<Button>();
        if (_skillButton == null)
        {
            Debug.LogError($"[SkillController] No Button component found on GameObject for skill {_skillType}");
            return;
        }

        if (_title == null)
        {
            Debug.LogError($"[SkillController] Title text not assigned for skill {_skillType}");
            return;
        }

        if (_glyph == null)
        {
            Debug.LogError($"[SkillController] Glyph code not assigned for skill {_skillType}");
            return;
        }

        // Subscribe to skill events
        _skillService.OnSkillLevelChanged += OnSkillLevelChanged;

        // Set up button click handler
        _skillButton.onClick.AddListener(OnSkillButtonClicked);

        // Initialize skill data
        _skillData = _skillService.GetSkillInfo(_skillType);
        if (_skillData == null)
        {
            Debug.LogError($"[SkillController] No skill data found for {_skillType}");
            return;
        }

        // Set initial title and glyph code
        SetSkillTitle();
        SetSkillIcon();
        UpdateLevelDisplay();

        // Update button state
        UpdateButtonState();
    }

    private void OnDestroy()
    {
        if (_skillService != null)
        {
            _skillService.OnSkillLevelChanged -= OnSkillLevelChanged;
        }

        if (_skillButton != null)
        {
            _skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        }
    }

    private void OnSkillLevelChanged(SkillType skill, int newLevel)
    {
        // Update button state when this skill changes or when any skill it depends on changes
        if (skill == _skillType || (_skillData != null && _skillData.RequiredSkills.Any(req => req.SkillType == skill)))
        {
            UpdateButtonState();
            UpdateLevelDisplay();
        }
    }

    private void OnSkillButtonClicked()
    {
        if (_skillService.CanLearnSkill(_skillType))
        {
            bool success = _skillService.LearnSkill(_skillType);
            if (success)
            {
                Debug.Log($"[SkillController] Skill {_skillType} learned successfully!");
            }
            else
            {
                Debug.LogWarning($"[SkillController] Failed to learn skill {_skillType}");
            }
        }
        else if (_skillService.CanLevelUpSkill(_skillType))
        {
            bool success = _skillService.LevelUpSkill(_skillType);
            if (success)
            {
                Debug.Log($"[SkillController] Skill {_skillType} leveled up successfully!");
            }
            else
            {
                Debug.LogWarning($"[SkillController] Failed to level up skill {_skillType}");
            }
        }
        else
        {
            Debug.Log($"[SkillController] Skill {_skillType} cannot be learned or leveled up yet");
        }
    }

    private void UpdateButtonState()
    {
        bool isLearned = _skillService.IsSkillLearned(_skillType); // Uses wrapper
        bool canLearn = _skillService.CanLearnSkill(_skillType);
        bool canLevelUp = _skillService.CanLevelUpSkill(_skillType); // Now same as CanLearnSkill

        // Update button interactability - can learn OR level up
        _skillButton.interactable = canLearn || canLevelUp;

        // Update visual appearance
        if (isLearned)
        {
            _glyph.color = _learnedColor;
        }
        else if (canLearn)
        {
            _glyph.color = _availableColor;
        }
        else
        {
            _glyph.color = _unavailableColor;
        }
        Debug.Log($"[SkillController] {_skillType} - Learned: {isLearned}, CanLearn: {canLearn}, CanLevelUp: {canLevelUp}, ButtonActive: {_skillButton.interactable}");
    }

    private void UpdateLevelDisplay()
    {
        if (_levelText == null) return;

        int currentLevel = _skillService.GetSkillLevel(_skillType);

        _levelText.text = currentLevel.ToString();
    }

    private void SetSkillTitle()
    {
        if (_skillData == null || string.IsNullOrEmpty(_skillData.DisplayName)) return;

        _title.text = _skillData.DisplayName;
    }

    private void SetSkillIcon()
    {
        if (_skillData == null || string.IsNullOrEmpty(_skillData.Glyph)) return;
        const string unicodePrefix = "\\u";
        _glyph.text = unicodePrefix + _skillData.Glyph;
    }

    // Public method to manually refresh the controller (useful for testing)
    [ContextMenu("Refresh Skill State")]
    public void RefreshSkillState()
    {
        if (_skillData != null)
        {
            UpdateButtonState();
            UpdateLevelDisplay();
        }
    }

    /// <summary>
    /// Force update of the controller state - useful when external changes occur
    /// </summary>
    public void ForceUpdate()
    {
        if (_skillData != null)
        {
            UpdateButtonState();
            UpdateLevelDisplay();
            SetSkillTitle();
            SetSkillIcon();
        }
    }

    /// <summary>
    /// Check if this skill can be learned right now
    /// </summary>
    public bool CanLearnSkill()
    {
        return _skillService?.CanLearnSkill(_skillType) ?? false;
    }

    /// <summary>
    /// Check if this skill is already learned
    /// </summary>
    public bool IsSkillLearned()
    {
        return _skillService?.IsSkillLearned(_skillType) ?? false;
    }
}
