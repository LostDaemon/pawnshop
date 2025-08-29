using System;
using System.Linq;
using System.Collections.Generic;
using PawnShop.Models;
using PawnShop.Models.Tags;

namespace PawnShop.Services
{
    public class EvaluationService : IEvaluationService
    {
        public long EvaluateByPlayer(ItemModel item, EvaluationStrategy strategy)
        {
            var tags = item.Tags.Where(tag => tag.IsRevealedToPlayer).ToList();
            return Evaluate(item, tags, strategy);
        }

        public long EvaluateByCustomer(ItemModel item, EvaluationStrategy strategy)
        {
            var tags = item.Tags.Where(tag => tag.IsRevealedToCustomer).ToList();
            return Evaluate(item, tags, strategy);
        }

        public long Evaluate(ItemModel item, List<BaseTagModel> tags, EvaluationStrategy strategy)
        {
            if (item == null) return 0;

            long finalPrice = item.BasePrice;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    bool shouldApplyTag = strategy switch
                    {
                        EvaluationStrategy.Realistic => true,
                        EvaluationStrategy.Pessimistic => tag.PriceMultiplier <= 1f,
                        EvaluationStrategy.Optimistic => tag.PriceMultiplier >= 1f,
                        _ => true
                    };

                    if (shouldApplyTag)
                    {
                        finalPrice = (long)(finalPrice * tag.PriceMultiplier);
                    }
                }
            }

            return Math.Max(0, finalPrice);
        }
    }
}
