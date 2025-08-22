using UnityEngine;

namespace PawnShop.ScriptableObjects.Tags
{
    [CreateAssetMenu(fileName = "TextTag", menuName = "ScriptableObjects/Tags/TextTag", order = 2)]
    public class TextTagPrototype : BaseTagPrototype
    {
        public string DefaultTextValue;
    }
}
