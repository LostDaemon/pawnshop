using System;
using PawnShop.Models.Tags;

namespace PawnShop.Models
{
    [Serializable]
    public class NegotiationRound
    {
        public int RoundId { get; set; }
        public BaseTagModel PlayerTag { get; set; }
        public BaseTagModel CustomerTag { get; set; }
        public float Effect { get; set; }
    }
}
