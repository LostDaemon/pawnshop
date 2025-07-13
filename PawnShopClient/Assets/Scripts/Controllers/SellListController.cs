using Zenject;

public class ListController : BaseListController
{
    private ISellService _sellService;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, ISellService sellService)
    {
        base.Construct(container, storageLocatorService);
        _sellService = sellService;
    }

    public void Schedule()
    {
        if (SelectedItem == null)
        {
            return;
        }

        SelectedItem.SellPrice = (int)(SelectedItem.PurchasePrice * 1.1f);
        _sellService.ScheduleForSale(SelectedItem);
        SelectedItem = null;
    }
}