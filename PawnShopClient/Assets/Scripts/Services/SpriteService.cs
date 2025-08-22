using UnityEngine;
using System.Collections.Generic;

namespace PawnShop.Services
{
    public class SpriteService : ISpriteService
    {
        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

        public SpriteService()
        {
            LoadBuiltInSprites();
        }

        public Sprite GetSprite(string id)
        {
            if (_sprites.TryGetValue(id, out var sprite))
            {
                return sprite;
            }

            Debug.LogWarning($"[SpriteService] Sprite '{id}' not found in dictionary or Resources");
            return null;
        }

        public void RegisterSprites(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0) return;

            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    _sprites[sprite.name] = sprite;
                    UnityEngine.Debug.Log($"[SpriteService] Registered sprite: {sprite.name}");
                }
            }
        }

        private void LoadBuiltInSprites()
        {
            var builtInSprites = Resources.LoadAll<Sprite>("Sprites/ItemsAtlas");
            foreach (var sprite in builtInSprites)
            {
                if (sprite != null)
                {
                    _sprites[sprite.name] = sprite;
                }
            }
            Debug.Log($"[SpriteService] Loaded {builtInSprites.Length} built-in sprites");
        }
    }
}