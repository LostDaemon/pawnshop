using PawnShop.Models;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Gamestate
{
    public class GameLoopState : IGameState
    {
        private readonly INegotiationService _negotiationService;

        public GameLoopState(
            INegotiationService purchaseService)
        {
            _negotiationService = purchaseService;
        }

        public void Enter()
        {
            _negotiationService.OnDealSuccess += OnDealSuccess;
            _negotiationService.OnSkipRequested += ShowNextCustomer;
            ShowNextCustomer();
        }

        public void Exit()
        {
            _negotiationService.OnDealSuccess -= OnDealSuccess;
            _negotiationService.OnSkipRequested -= ShowNextCustomer;
        }

        private void ShowNextCustomer()
        {
            _negotiationService.ShowNextCustomer();
        }

        private void OnDealSuccess()
        {
            Debug.Log("[GameLoop] Deal successful.");
            ShowNextCustomer();
        }
    }
}