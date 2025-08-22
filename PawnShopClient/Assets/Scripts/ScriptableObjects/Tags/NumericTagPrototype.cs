using UnityEngine;

namespace PawnShop.ScriptableObjects.Tags
{
    [CreateAssetMenu(fileName = "NumericTag", menuName = "ScriptableObjects/Tags/NumericTag", order = 3)]
    public class NumericTagPrototype : BaseTagPrototype
    {
        public float DefaultNumericValue;
    }
}
