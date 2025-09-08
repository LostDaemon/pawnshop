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
            // Don't show customer immediately - wait for scheduled events
            // ShowNextCustomer(); // Removed - customers will come via event system
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