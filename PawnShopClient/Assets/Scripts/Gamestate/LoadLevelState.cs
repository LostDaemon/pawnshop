using Zenject;

public class LoadLevelState : IGameState
{
    private ISceneLoader _sceneLoader;
    private GameStateMachine _stateMachine;

    [Inject]
    public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader)
    {
        _stateMachine = stateMachine;
        _sceneLoader = sceneLoader;
    }

    public void Enter()
    {
        _sceneLoader.Load("MainScene", () =>
        {
            _stateMachine.Enter<GameLoopState>();
        });
    }

    public void Exit() { }
}