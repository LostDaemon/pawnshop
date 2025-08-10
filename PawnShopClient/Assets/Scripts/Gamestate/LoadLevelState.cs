using Zenject;

public class LoadLevelState : IGameState
{
    private ISceneLoader _sceneLoader;
    private GameStateMachine _stateMachine;
    private IItemRepositoryService _itemRepositoryService;
    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepositoryService itemRepositoryService)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
        _itemRepositoryService = itemRepositoryService;
    }

    public void Enter()
    {
        _itemRepositoryService.Load();
        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}