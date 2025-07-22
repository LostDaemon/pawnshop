using Zenject;

public class SellListController : BaseListController
{
    private ISellService _sellService;

    [Inject]
    public void Construct(DiContainer container, IStorageLocatorService storageLocatorService, ISellService sellService)
    {
        base.Construct(container, storageLocatorService);
        _sellService = sellService;
    }

    public void Schedule10prcDiscount()
    {
        Schedule(0.9m);
    }

    public void ScheduleAsIs()
    {
        Schedule(1m);
    }

    public void Schedule10prc()
    {
        Schedule(1.1m);
    }

    public void Schedule25prc()
    {
        Schedule(1.25m);
    }

    public void Schedule50prc()
    {
        Schedule(1.5m);
    }

    private void Schedule(decimal multiplier = 1m)
    {
        if (SelectedItem == null)
        {
            return;
        }

        SelectedItem.SellPrice = (long)(SelectedItem.PurchasePrice * multiplier);
        _sellService.ScheduleForSale(SelectedItem);
        SelectedItem = null;
    }
}