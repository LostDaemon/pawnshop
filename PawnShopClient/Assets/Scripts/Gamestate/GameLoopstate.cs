using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Gamestate
{
    public class GameLoopState : IGameState
    {
        private readonly ICustomerService _customerService;

        public GameLoopState(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public void Enter()
        {
            NextCustomer();
        }

        public void Exit()
        {
            _customerService.ClearCustomer();
        }

        private void NextCustomer()
        {
            _customerService.NextCustomer();
        }
    }
}