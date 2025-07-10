using System;
using System.Collections.Generic;

public interface ISellService
{
    int MaxSlots { get; }
    IReadOnlyList<ItemModel> DisplayedItems { get; }
    IReadOnlyDictionary<ItemModel, GameTime> ScheduledSales { get; }

    event Action<ItemModel> OnStartSelling;
    event Action<ItemModel> OnSold;

    void ConfigureSlots(int count);
    bool RemoveFromSelling(ItemModel item);
    bool ScheduleForSale(ItemModel item);
}