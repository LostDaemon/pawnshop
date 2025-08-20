using System;
using System.Linq;

public class EvaluationService : IEvaluationService
{
    public long Evaluate(ICharacter character, ItemModel item)
    {
        if (item == null) return 0;

        long finalPrice = item.BasePrice;

        bool isPlayer = character is Player;
        bool isCustomer = character is Customer;

        if (item.Tags != null)
        {
            foreach (var tag in item.Tags)
            {
                bool isTagRevealed = false;
                if (isPlayer)
                {
                    isTagRevealed = tag.IsRevealedToPlayer;
                }
                else if (isCustomer)
                {
                    isTagRevealed = tag.IsRevealedToCustomer;
                }

                if (isTagRevealed)
                {
                    finalPrice = (long)(finalPrice * tag.PriceMultiplier);
                }
            }
        }

        return Math.Max(0, finalPrice);
    }
}
