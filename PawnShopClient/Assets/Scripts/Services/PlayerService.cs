using System.Collections.Generic;
using Zenject;

public class PlayerService : IPlayerService
{
    private readonly ISkillRepositoryService _skillRepository;

    public Player Player { get; private set; }

    [Inject]
    public PlayerService(ISkillRepositoryService skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public void InitializePlayer()
    {
        Player = new Player();
        InitializePlayerSkills();
    }

    private void InitializePlayerSkills()
    {
        // Initialize all skills as not learned
        foreach (SkillType skillType in System.Enum.GetValues(typeof(SkillType)))
        {
            if (skillType != SkillType.Undefined)
            {
                var prototype = _skillRepository.GetSkill(skillType);
                if (prototype != null)
                {
                    Player.Skills[skillType] = new Skill(prototype);
                    Player.Skills[skillType].Level = 2; // TODO: Load from config later
                    UnityEngine.Debug.LogWarning($"[PlayerService] Skill {skillType} initialized with level {Player.Skills[skillType].Level}.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[PlayerService] No prototype found for skill {skillType}");
                }
            }
        }

        UnityEngine.Debug.Log($"[PlayerService] Player initialized with {Player.Skills.Count} skills.");
    }
}
