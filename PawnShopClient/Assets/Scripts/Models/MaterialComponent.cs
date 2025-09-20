using UnityEngine;

namespace PawnShop.Models
{
    [System.Serializable]
    public class MaterialComponent
    {
        [Header("Material Information")]
        public CurrencyType MaterialType;
        public int Quantity;
        
        public MaterialComponent()
        {
            MaterialType = CurrencyType.Undefined;
            Quantity = 0;
        }
        
        public MaterialComponent(CurrencyType materialType, int quantity)
        {
            MaterialType = materialType;
            Quantity = quantity;
        }
    }
}
