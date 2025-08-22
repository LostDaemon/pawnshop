using PawnShop.Gamestate;
using UnityEngine;
using Zenject;

namespace PawnShop.Infrastructure
{
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
}