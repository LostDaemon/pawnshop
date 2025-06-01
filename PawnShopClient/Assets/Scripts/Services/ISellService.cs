using System;
using System.Collections.Generic;

public interface ISellService
{
    int MaxSlots { get; }
    IReadOnlyList<ItemModel> DisplayedItems { get; }

    void ConfigureSlots(int count);
    void TryAutoFillDisplay();

    bool SellItem(ItemModel item);
    bool RemoveFromDisplay(ItemModel item);

    event Action OnDisplayUpdated;
    event Action<ItemModel> OnSold;
}