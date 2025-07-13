public class ItemProcessingService : IItemProcessingService
{
    public void Process(ItemModel item, ItemProcess itemProcess)
    {
        switch (itemProcess)
        {
            case ItemProcess.Inspect:
                item.Inspected = true;
                break;
        }
    }
}