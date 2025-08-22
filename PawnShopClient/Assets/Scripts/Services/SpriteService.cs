using System.Linq;
using UnityEngine;

namespace PawnShop.Services
{
    public class SpriteService : ISpriteService
    {
        private readonly string _atlasPath;
        private static Sprite[] _cachedSprites;

        public SpriteService(string atlasPath)
        {
            _atlasPath = atlasPath;
            if (_cachedSprites == null)
                _cachedSprites = Resources.LoadAll<Sprite>(_atlasPath);
        }

        public Sprite GetSprite(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("Sprite ID is null or empty.");
                return null;
            }

            var result = _cachedSprites.FirstOrDefault(s => s.name == id);
            if (result == null)
            {
                Debug.LogWarning($"Sprite '{id}' not found in atlas '{_atlasPath}'.");
            }

            return result;
        }
    }
}