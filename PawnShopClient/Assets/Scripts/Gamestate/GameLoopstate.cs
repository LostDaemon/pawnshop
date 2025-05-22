using UnityEngine;

public class GameLoopState : IGameState
{
    private readonly INegotiateService _purchaseService;
    private readonly ICustomerFactoryService _customerFactory;

    public GameLoopState(
        INegotiateService purchaseService,
        ICustomerFactoryService customerFactory)
    {
        _purchaseService = purchaseService;
        _customerFactory = customerFactory;
    }

    public void Enter()
    {
        _purchaseService.OnPurchased += OnItemPurchased;
        _purchaseService.OnSkipRequested += ShowNextCustomer;
        ShowNextCustomer();
    }

    public void Exit()
    {
        _purchaseService.OnPurchased -= OnItemPurchased;
        _purchaseService.OnSkipRequested -= ShowNextCustomer;
    }

    private void ShowNextCustomer()
    {
        var customer = _customerFactory.GenerateRandomCustomer();
        if (customer.OwnedItem != null && customer.OwnedItem.IsFake)
        {
            Debug.Log("[GameLoop] Item is fake.");
        }

        _purchaseService.SetCurrentCustomer(customer);
    }

    private void OnItemPurchased(ItemModel _)
    {
        Debug.Log("[GameLoop] Item purchased.");
        ShowNextCustomer();
    }
}