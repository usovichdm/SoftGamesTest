using System.Collections.Generic;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Raw dialogue payload before emoji parsing.
    /// </summary>
    public sealed class DialogueMessage
    {
        public string Text { get; }

        public DialogueMessage(string text)
        {
            Text = text ?? string.Empty;
        }
    }

    public abstract class DialogueToken
    {
    }

    public sealed class TextToken : DialogueToken
    {
        public string Value { get; }

        public TextToken(string value)
        {
            Value = value ?? string.Empty;
        }
    }

    public sealed class EmojiToken : DialogueToken
    {
        public string Key { get; }

        public string Unicode { get; }

        public string AtlasId { get; }

        public EmojiToken(string key, string unicode, string atlasId)
        {
            Key = key ?? string.Empty;
            Unicode = unicode ?? string.Empty;
            AtlasId = atlasId ?? string.Empty;
        }
    }

    /// <summary>
    /// Maps SoftGames {token} keys to Unicode + EmojiOne atlas frame ids.
    /// </summary>
    public static class EmojiCatalog
    {
        public readonly struct Entry
        {
            public readonly string Unicode;
            public readonly string AtlasId;

            public Entry(string unicode, string atlasId)
            {
                Unicode = unicode;
                AtlasId = atlasId;
            }
        }

        private static readonly Dictionary<string, Entry> Map = new Dictionary<string, Entry>(16)
        {
            { "satisfied", new Entry("😌", "1f60a") },
            { "intrigued", new Entry("🤔", "1f609") },
            { "neutral", new Entry("😐", "263a") },
            { "affirmative", new Entry("👍", "1f60e") },
            { "laughing", new Entry("😂", "1f602") },
            { "win", new Entry("🏆", "1f601") },
            { "happy", new Entry("😊", "1f60a") },
            { "sad", new Entry("😢", "2639") },
            { "angry", new Entry("😠", "2639") },
            { "love", new Entry("❤️", "1f618") },
            { "fire", new Entry("🔥", "1f923") },
            { "cool", new Entry("😎", "1f60e") },
        };

        public static bool TryGet(string key, out Entry entry)
        {
            if (string.IsNullOrEmpty(key))
            {
                entry = default;
                return false;
            }

            return Map.TryGetValue(key, out entry);
        }
    }
}
