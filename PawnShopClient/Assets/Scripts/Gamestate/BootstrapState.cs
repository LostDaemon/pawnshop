using Zenject;

namespace PawnShop.Gamestate
{
    public class BootstrapState : IGameState
    {
        private GameStateMachine _stateMachine;

        [Inject]
        public void Construct(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _stateMachine.Enter<LoadLevelState>();
        }

        public void Exit() { }
    }
}