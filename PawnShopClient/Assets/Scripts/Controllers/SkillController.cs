using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public class SkillController : MonoBehaviour
{
    [Header("Skill Configuration")]
    [SerializeField] private SkillType _skillType;
    [SerializeField] private Button _skillButton;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _glyph;

    [Header("Visual States")]
    [SerializeField] private Color _learnedColor = Color.white;
    [SerializeField] private Color _unavailableColor = Color.gray;
    [SerializeField] private Color _availableColor = Color.white;

    [Inject] private ISkillService _skillService;

    private Skill _skillData;

    [Inject]
    public void Construct(ISkillService skillService)
    {
        _skillService = skillService;
    }

    private void Start()
    {
        if (_skillButton == null)
        {
            Debug.LogError($"[SkillController] Skill button not assigned for skill {_skillType}");
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
        _skillService.OnSkillStatusChanged += OnSkillStatusChanged;
        _skillService.OnSkillLearned += OnSkillLearned;

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

        if (_skillButton != null)
        {
            _skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        }
    }

    private void OnSkillStatusChanged(SkillType skill, bool isLearned)
    {
        if (skill == _skillType)
        {
            UpdateButtonState();
        }
    }

    private void OnSkillLearned(SkillType skill)
    {
        if (skill == _skillType)
        {
            UpdateButtonState();
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
        else
        {
            Debug.Log($"[SkillController] Skill {_skillType} cannot be learned yet");
        }
    }

    private void UpdateButtonState()
    {
        if (_skillData == null) return;

        bool isLearned = _skillService.IsSkillLearned(_skillType);
        bool canLearn = _skillService.CanLearnSkill(_skillType);

        // Update button interactability
        _skillButton.interactable = canLearn && !isLearned;

        // Update visual appearance
        if (isLearned)
        {
            // Skill is already learned - bright icon, button disabled
            _glyph.color = _learnedColor;
            _skillButton.interactable = false;
        }
        else if (canLearn)
        {
            // Skill can be learned - bright icon, button enabled
            _glyph.color = _availableColor;
            _skillButton.interactable = true;
        }
        else
        {
            // Skill cannot be learned yet - gray icon, button disabled
            _glyph.color = _unavailableColor;
            _skillButton.interactable = false;
        }
    }

    private void SetSkillTitle()
    {
        if (_skillData == null || string.IsNullOrEmpty(_skillData.DisplayName)) return;

        _title.text = _skillData.DisplayName;
    }

    private void SetSkillIcon()
    {
        if (_skillData == null || string.IsNullOrEmpty(_skillData.Icon)) return;
        const string unicodePrefix = "\\u";
        _glyph.text = unicodePrefix + _skillData.Icon;
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
