using UnityEngine;

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
        _negotiationService.OnPurchased += OnItemPurchased;
        _negotiationService.OnSkipRequested += ShowNextCustomer;
        ShowNextCustomer();
    }

    public void Exit()
    {
        _negotiationService.OnPurchased -= OnItemPurchased;
        _negotiationService.OnSkipRequested -= ShowNextCustomer;
    }

    private void ShowNextCustomer()
    {
        _negotiationService.ShowNextCustomer();
    }

    private void OnItemPurchased(ItemModel _)
    {
        Debug.Log("[GameLoop] Item purchased.");
        ShowNextCustomer();
    }
}