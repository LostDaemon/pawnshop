using Zenject;

public class LoadLevelState : IGameState
{
    private ISceneLoader _sceneLoader;
    private GameStateMachine _stateMachine;
    private IItemRepositoryService _itemRepositoryService;
    private ISkillRepositoryService _skillRepositoryService;
    private ITagRepositoryService _tagRepositoryService;
    private ILocalizationService _localizationService;
    private ILanguageRepositoryService _languageRepositoryService;

    
    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepositoryService itemRepositoryService, ISkillRepositoryService skillRepositoryService, ITagRepositoryService tagRepositoryService, ILocalizationService localizationService, ILanguageRepositoryService languageRepositoryService)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
        _itemRepositoryService = itemRepositoryService;
        _skillRepositoryService = skillRepositoryService;
        _tagRepositoryService = tagRepositoryService;
        _localizationService = localizationService;
        _languageRepositoryService = languageRepositoryService;

    }

    public void Enter()
    {
        _itemRepositoryService.Load();
        _skillRepositoryService.Load();
        _tagRepositoryService.Load();
        _languageRepositoryService.Load();

        // Set default language after loading language prototypes
        _localizationService.SwitchLocalization(Language.Russian);

        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}