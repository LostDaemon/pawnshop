using UnityEngine;

namespace PawnShop.Services
{
    public interface ISpriteService
    {
        Sprite GetSprite(string id);
        void RegisterSprites(Sprite[] sprites);
    }
}