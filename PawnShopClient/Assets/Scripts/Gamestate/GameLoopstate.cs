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
        ShowNextItem();
    }

    public void Exit()
    {
        _purchaseService.OnPurchased -= OnItemPurchased;
    }

    private void ShowNextItem()
    {
        var item = _itemRepository.GetRandomItem();
        _purchaseService.SetCurrentItem(item);
    }

    private void OnItemPurchased(ItemModel _)
    {
        ShowNextItem();
    }
}