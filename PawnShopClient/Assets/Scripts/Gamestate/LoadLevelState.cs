using Zenject;

public class LoadLevelState : IGameState
{
    private ISceneLoader _sceneLoader;
    private GameStateMachine _stateMachine;
    private IItemRepositoryService _itemRepositoryService;
    private ISkillRepositoryService _skillRepositoryService;
    
    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepositoryService itemRepositoryService, ISkillRepositoryService skillRepositoryService)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
        _itemRepositoryService = itemRepositoryService;
        _skillRepositoryService = skillRepositoryService;
    }

    public void Enter()
    {
        _itemRepositoryService.Load();
        _skillRepositoryService.Load();
        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}