using System.Collections.Generic;
using PawnShop.Models;

namespace PawnShop.Models.Characters
{
    public class Customer : BaseCharacter
    {
        public ItemModel OwnedItem { get; set; }
        public CustomerType CustomerType { get; set; }
        public float Patience { get; set; } = 100f; // Customer patience level (0-100)
    }
}