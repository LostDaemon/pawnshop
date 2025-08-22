using PawnShop.ScriptableObjects.Tags;

namespace PawnShop.Models.Tags
{
    public class NumericTagModel : BaseTagModel
    {
        public float NumericValue { get; set; }

        public NumericTagModel(NumericTagPrototype prototype) : base(prototype)
        {
            if (prototype != null)
            {
                NumericValue = prototype.DefaultNumericValue;
            }
        }
    }
}
