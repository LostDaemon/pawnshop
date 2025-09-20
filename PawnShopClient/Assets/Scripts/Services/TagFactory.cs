using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects.Tags;

namespace PawnShop.Services
{
    public class TagFactory : ITagFactory
    {
        public BaseTagModel Create(BaseTagPrototype prototype)
        {
            if (prototype == null) return null;

            return prototype switch
            {
                SimpleTagPrototype simplePrototype => new SimpleTagModel(simplePrototype),
                TextTagPrototype textPrototype => new TextTagModel(textPrototype),
                NumericTagPrototype numericPrototype => new NumericTagModel(numericPrototype),
                _ => null
            };
        }
    }
}
