using Zenject;

public class ItemLoaderController : BaseLoader<ItemPrototype>
{
    private IItemRepositoryService _itemrepositoryService;

    [Inject]
    public void Construct(IItemRepositoryService itemRepositoryService)
    {
        _itemrepositoryService = itemRepositoryService;
    }

    protected override void Load(ItemPrototype prototype)
    {
        _itemrepositoryService.AddItem(prototype);
    }
}

