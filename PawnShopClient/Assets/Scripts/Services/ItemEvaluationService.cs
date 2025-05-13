using UnityEngine;

public class ItemEvaluatorService : IItemEvaluationService
{
    private const float FairThreshold = 0.1f; // ±10% of real price

    public EvaluationResult Evaluate(ItemModel item, int offeredPrice)
    {
        var difference = Mathf.Abs(item.RealPrice - offeredPrice);
        var margin = item.RealPrice * FairThreshold;

        if (difference <= margin)
            return EvaluationResult.Fair;

        return offeredPrice < item.RealPrice
            ? EvaluationResult.TooLow
            : EvaluationResult.TooHigh;
    }
}