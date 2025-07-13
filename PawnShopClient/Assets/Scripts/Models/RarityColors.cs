using System.Collections.Generic;
using UnityEngine;

public static class RarityColors
{
    private static readonly Dictionary<ItemRarity, Color> _rarityColorMap = new()
        {
            {ItemRarity.Undefined, new Color(0,0,0)},
            {ItemRarity.Trash, new Color(128/255f,128/255f,128/255f)},
            {ItemRarity.Common, new Color(1f,1f,1f)},
            {ItemRarity.Uncommon, new Color(68/255f,190/255f,67/255f)},
            {ItemRarity.Rare,  new Color(8/255f,12/255f,210/255f)},
            {ItemRarity.Unique, new Color(113/255f,16/255f,154/255f)},
            {ItemRarity.Relic, new Color(232/255f,144/255f,40/255f)}
        };


    public static Color GetColor(ItemRarity rarity)
    {
        return _rarityColorMap[rarity];
    }
}
