using System;
using System.Linq;
using System.Collections.Generic;

public class EvaluationService : IEvaluationService
{
    public long EvaluateByPlayer(ItemModel item)
    {
        var tags = item.Tags.Where(tag => tag.IsRevealedToPlayer).ToList();
        return Evaluate(item, tags);
    }

    public long EvaluateByCustomer(ItemModel item)
    {
        var tags = item.Tags.Where(tag => tag.IsRevealedToCustomer).ToList();
        return Evaluate(item, tags);
    }

    public long Evaluate(ItemModel item, List<BaseTagModel> tags)
    {
        if (item == null) return 0;

        long finalPrice = item.BasePrice;


        if (tags != null)
        {
            foreach (var tag in tags)
            {
                finalPrice = (long)(finalPrice * tag.PriceMultiplier);
            }
        }



        return Math.Max(0, finalPrice);
    }
}
