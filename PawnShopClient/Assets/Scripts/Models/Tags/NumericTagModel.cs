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
