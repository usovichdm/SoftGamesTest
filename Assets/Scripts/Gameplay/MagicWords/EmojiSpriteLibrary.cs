using System.Collections.Generic;
using UnityEngine;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Builds UI sprites from the EmojiOne atlas for known SoftGames tokens.
    /// </summary>
    public sealed class EmojiSpriteLibrary
    {
        private static EmojiSpriteLibrary _shared;

        private readonly Dictionary<string, Sprite> _byAtlasId = new Dictionary<string, Sprite>(16);

        public static EmojiSpriteLibrary Shared
        {
            get
            {
                if (_shared == null)
                {
                    var atlas = Resources.Load<Texture2D>("Sprites/EmojiOne");
                    _shared = new EmojiSpriteLibrary(atlas);
                }

                return _shared;
            }
        }

        public EmojiSpriteLibrary(Texture2D atlas)
        {
            if (atlas == null)
            {
                return;
            }

            // Coordinates match Assets/TextMesh Pro/Sprites/EmojiOne.json (y converted to Unity bottom-left).
            var h = atlas.height;
            Add(atlas, "1f60a", 0, h - 128);
            Add(atlas, "1f60b", 128, h - 128);
            Add(atlas, "1f60d", 256, h - 128);
            Add(atlas, "1f60e", 384, h - 128);
            Add(atlas, "1f600", 0, h - 256);
            Add(atlas, "1f601", 128, h - 256);
            Add(atlas, "1f602", 256, h - 256);
            Add(atlas, "1f603", 384, h - 256);
            Add(atlas, "1f604", 0, h - 384);
            Add(atlas, "1f605", 128, h - 384);
            Add(atlas, "1f606", 256, h - 384);
            Add(atlas, "1f609", 384, h - 384);
            Add(atlas, "1f618", 0, h - 512);
            Add(atlas, "1f923", 128, h - 512);
            Add(atlas, "263a", 256, h - 512);
            Add(atlas, "2639", 384, h - 512);
        }

        public Sprite GetSprite(EmojiToken token)
        {
            if (token == null || string.IsNullOrEmpty(token.AtlasId))
            {
                return null;
            }

            return _byAtlasId.TryGetValue(token.AtlasId, out var sprite) ? sprite : null;
        }

        private void Add(Texture2D atlas, string id, int x, int y)
        {
            if (y < 0)
            {
                y = 0;
            }

            var sprite = Sprite.Create(
                atlas,
                new Rect(x, y, 128f, 128f),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = id;
            _byAtlasId[id] = sprite;
        }
    }
}
