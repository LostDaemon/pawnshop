public interface IEvaluationService
{
    /// <summary>
    /// Evaluate an item and return its estimated value
    /// </summary>
    /// <param name="character">Character performing the evaluation</param>
    /// <param name="item">Item to be evaluated</param>
    /// <returns>Estimated value of the item</returns>
    long Evaluate(ICharacter character, ItemModel item);
}
