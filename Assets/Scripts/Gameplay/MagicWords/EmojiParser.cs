using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Splits a dialogue message into text / emoji tokens. Unknown {tokens} stay as text.
    /// </summary>
    public sealed class EmojiParser
    {
        private static readonly Regex TokenRegex = new Regex(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

        public List<DialogueToken> Parse(DialogueMessage message)
        {
            var tokens = new List<DialogueToken>(8);
            var raw = message != null ? message.Text : null;
            if (string.IsNullOrEmpty(raw))
            {
                return tokens;
            }

            var matches = TokenRegex.Matches(raw);
            if (matches.Count == 0)
            {
                tokens.Add(new TextToken(raw));
                return tokens;
            }

            var cursor = 0;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (match.Index > cursor)
                {
                    tokens.Add(new TextToken(raw.Substring(cursor, match.Index - cursor)));
                }

                var key = match.Groups[1].Value;
                if (EmojiCatalog.TryGet(key, out var entry))
                {
                    tokens.Add(new EmojiToken(key, entry.Unicode, entry.AtlasId));
                }
                else
                {
                    tokens.Add(new TextToken(match.Value));
                }

                cursor = match.Index + match.Length;
            }

            if (cursor < raw.Length)
            {
                tokens.Add(new TextToken(raw.Substring(cursor)));
            }

            return tokens;
        }
    }
}
