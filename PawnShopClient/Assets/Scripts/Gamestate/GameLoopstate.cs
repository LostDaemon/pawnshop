using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Gamestate
{
    public class GameLoopState : IGameState
    {
        private readonly INegotiationService _negotiationService;
        private readonly ICustomerService _customerService;

        public GameLoopState(INegotiationService negotiationService, ICustomerService customerService)
        {
            _negotiationService = negotiationService;
            _customerService = customerService;
        }

        public void Enter()
        {
            _negotiationService.OnDealSuccess += NextCustomer;
            _negotiationService.OnSkipRequested += NextCustomer;
            NextCustomer();
        }

        public void Exit()
        {
            _negotiationService.OnDealSuccess -= NextCustomer;
            _negotiationService.OnSkipRequested -= NextCustomer;
        }

        private void NextCustomer()
        {
            _customerService.NextCustomer();
        }
    }
}