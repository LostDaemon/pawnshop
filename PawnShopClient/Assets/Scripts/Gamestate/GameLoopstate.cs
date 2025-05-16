using UnityEngine;
using Zenject;

public class GameLoopState : IGameState
{
    private readonly IItemRepositoryService _itemRepository;
    private readonly IPurchaseService _purchaseService;

    public GameLoopState(
        IItemRepositoryService itemRepository,
        IPurchaseService purchaseService)
    {
        _itemRepository = itemRepository;
        _purchaseService = purchaseService;
    }

    public void Enter()
    {
        _purchaseService.OnPurchased += OnItemPurchased;
        _purchaseService.OnSkipRequested += ShowNextItem;
        ShowNextItem();
    }

    public void Exit()
    {
        _purchaseService.OnPurchased -= OnItemPurchased;
        _purchaseService.OnSkipRequested -= ShowNextItem;
    }

    private void ShowNextItem()
    {
        var item = _itemRepository.GetRandomItem();
        _purchaseService.SetCurrentItem(item);
    }

    private void OnItemPurchased(ItemModel _)
    {
        Debug.Log("[GameLoop] Player skipped the item.");
        ShowNextItem();
    }
}