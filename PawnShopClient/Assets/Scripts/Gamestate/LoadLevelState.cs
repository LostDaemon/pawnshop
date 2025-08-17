using Zenject;

public class LoadLevelState : IGameState
{
    private ISceneLoader _sceneLoader;
    private GameStateMachine _stateMachine;
    private IItemRepositoryService _itemRepositoryService;
    private ISkillRepositoryService _skillRepositoryService;
    private ITagRepositoryService _tagRepositoryService;
    
    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepositoryService itemRepositoryService, ISkillRepositoryService skillRepositoryService, ITagRepositoryService tagRepositoryService)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
        _itemRepositoryService = itemRepositoryService;
        _skillRepositoryService = skillRepositoryService;
        _tagRepositoryService = tagRepositoryService;
    }

    public void Enter()
    {
        _itemRepositoryService.Load();
        _skillRepositoryService.Load();
        _tagRepositoryService.Load();
        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}