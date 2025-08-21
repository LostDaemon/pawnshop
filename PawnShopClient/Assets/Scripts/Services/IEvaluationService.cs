using System.Collections.Generic;

public interface IEvaluationService
{
    /// <summary>
    /// Evaluate an item based on tags revealed to player
    /// </summary>
    /// <param name="item">Item to be evaluated</param>
    /// <returns>Estimated value of the item from player's perspective</returns>
    long EvaluateByPlayer(ItemModel item);

    /// <summary>
    /// Evaluate an item based on tags revealed to customer
    /// </summary>
    /// <param name="item">Item to be evaluated</param>
    /// <returns>Estimated value of the item from customer's perspective</returns>
    long EvaluateByCustomer(ItemModel item);

    /// <summary>
    /// Evaluate an item based on provided tags
    /// </summary>
    /// <param name="item">Item to be evaluated</param>
    /// <param name="tags">List of tags</param>
    /// <returns>Estimated value of the item based on BaseCost corrected by tags provided</returns>
    long Evaluate(ItemModel item, List<BaseTagModel> tags);
}
