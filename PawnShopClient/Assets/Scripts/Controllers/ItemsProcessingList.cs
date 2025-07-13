using Zenject;

public class ItemsProcessingList : BaseListController
{
    private IItemProcessingService _itemProcessingService;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, IItemProcessingService itemProcessingService)
    {
        base.Construct(container, storageLocatorService);
        _itemProcessingService = itemProcessingService;
    }

    public void Inspect()
    {
        if (SelectedItem == null)
        {
            return;
        }

        _itemProcessingService.Process(SelectedItem, ItemProcess.Inspect);
        SelectedItem = null;
    }
}