using PawnShop.Models.Npc;

namespace PawnShop.Models.Characters
{
    public class Customer : BaseCharacter
    {
        public ItemModel OwnedItem { get; set; }
        public NpcType CustomerType { get; set; }
        public float Patience { get; set; } = 100f;
    }
}