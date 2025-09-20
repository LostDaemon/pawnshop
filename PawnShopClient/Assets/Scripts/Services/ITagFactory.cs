using PawnShop.Models.Tags;
using PawnShop.ScriptableObjects.Tags;

namespace PawnShop.Services
{
    public interface ITagFactory
    {
        BaseTagModel Create(BaseTagPrototype prototype);
    }
}
