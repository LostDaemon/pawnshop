
using System.Collections.Generic;
using PawnShop.Models.Tags;
using UnityEngine;

namespace PawnShop.Models
{
    public class ItemModel
    {
        public string Id { get; set; }
        public string ClassId { get; set; }
        public long PurchasePrice { get; set; }
        public long SellPrice { get; set; }
        public bool IsFake { get; set; }
        public int Condition { get; set; }
        public bool Inspected { get; set; } //TODO: Temporary variable. To be replaced by "tags" [{TagType, Revealed}]
        public string Name { get; set; }
        public Sprite Image { get; set; }
        public long BasePrice { get; set; }
        public float Scale { get; set; }
        public string Description { get; set; }
        public long CurrentOffer { get; set; }

        // Tags system
        public List<BaseTagModel> Tags { get; set; } = new List<BaseTagModel>();
        
        // Materials system
        public List<MaterialComponent> Materials { get; set; } = new List<MaterialComponent>();
    }
}
