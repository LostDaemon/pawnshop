using UnityEngine;
using Zenject;

public class GameBootstrapper : MonoBehaviour
{
    private GameStateMachine _stateMachine;

    [Inject]
    public void Construct(GameStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    private void Start()
    {
        _stateMachine.Enter<BootstrapState>();
    }
}