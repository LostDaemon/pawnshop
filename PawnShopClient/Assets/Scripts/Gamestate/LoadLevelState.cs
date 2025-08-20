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
    private IPlayerService _playerService;


    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepositoryService itemRepositoryService, ISkillRepositoryService skillRepositoryService, ITagRepositoryService tagRepositoryService, ILocalizationService localizationService, ILanguageRepositoryService languageRepositoryService, IPlayerService playerService)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
        _itemRepositoryService = itemRepositoryService;
        _skillRepositoryService = skillRepositoryService;
        _tagRepositoryService = tagRepositoryService;
        _localizationService = localizationService;
        _languageRepositoryService = languageRepositoryService;
        _playerService = playerService;
    }

    public void Enter()
    {
        _itemRepositoryService.Load();
        _skillRepositoryService.Load();
        _tagRepositoryService.Load();
        _languageRepositoryService.Load();

        // Initialize player after loading skill prototypes
        _playerService.InitializePlayer();

        // Set default language after loading language prototypes
        _localizationService.SwitchLocalization(Language.Russian);

        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}