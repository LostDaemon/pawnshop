public interface IItemEvaluationService
{
    EvaluationResult Evaluate(ItemModel item, int offeredPrice);
}