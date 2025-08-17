public class TextTagModel : BaseTagModel
{
    public string TextValue { get; set; }

    public TextTagModel(TextTagPrototype prototype) : base(prototype)
    {
        if (prototype != null)
        {
            TextValue = prototype.DefaultTextValue;
        }
    }
}
