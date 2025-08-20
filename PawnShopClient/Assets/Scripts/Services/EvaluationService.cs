using System;
using System.Linq;
using UnityEngine;

public class EvaluationService : IEvaluationService
{
    public long Evaluate(ICharacter character, ItemModel item)
    {
        if (item == null) return 0;

        // Start with base price
        long finalPrice = item.BasePrice;

        // Apply modifiers from revealed tags
        if (item.Tags != null)
        {
            foreach (var tag in item.Tags)
            {
                if (tag.IsRevealed)
                {
                    finalPrice = (long)(finalPrice * tag.PriceMultiplier);
                }
            }
        }

        return Math.Max(0, finalPrice);
    }
}
