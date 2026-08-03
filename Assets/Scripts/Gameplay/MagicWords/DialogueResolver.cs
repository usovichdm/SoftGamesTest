using System;
using System.Collections.Generic;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Pure dialogue resolution: avatar map merge + line binding without Unity/UI.
    /// </summary>
    public sealed class DialogueResolver
    {
        private readonly EmojiParser _parser;

        public DialogueResolver(EmojiParser parser = null)
        {
            _parser = parser ?? new EmojiParser();
        }

        public List<ResolvedDialogueLine> BuildLines(MagicWordsResponse response)
        {
            var avatars = BuildAvatarMap(response != null ? response.avatars : null);
            var source = response != null && response.dialogue != null
                ? response.dialogue
                : Array.Empty<DialogueEntry>();

            var lines = new List<ResolvedDialogueLine>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var entry = source[i];
                if (entry == null)
                {
                    continue;
                }

                var speaker = string.IsNullOrWhiteSpace(entry.name) ? "Unknown" : entry.name.Trim();
                avatars.TryGetValue(speaker, out var avatar);

                var message = new DialogueMessage(entry.text);
                var tokens = _parser.Parse(message);

                lines.Add(new ResolvedDialogueLine
                {
                    Speaker = speaker,
                    Message = message,
                    Tokens = tokens,
                    Avatar = avatar,
                    Side = avatar != null ? avatar.Side : AvatarSide.Left
                });
            }

            return lines;
        }

        public static Dictionary<string, ResolvedAvatar> BuildAvatarMap(AvatarEntry[] entries)
        {
            var map = new Dictionary<string, ResolvedAvatar>(8);
            if (entries == null)
            {
                return map;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.name))
                {
                    continue;
                }

                var name = entry.name.Trim();
                var url = entry.url != null ? entry.url.Trim() : string.Empty;
                var side = ParseSide(entry.position);

                if (map.TryGetValue(name, out var existing))
                {
                    // First non-empty URL wins; later empty entries do not wipe it.
                    if (string.IsNullOrEmpty(existing.Url) && !string.IsNullOrEmpty(url))
                    {
                        existing.Url = url;
                        existing.Side = side;
                    }

                    continue;
                }

                map[name] = new ResolvedAvatar
                {
                    Name = name,
                    Url = url,
                    Side = side
                };
            }

            return map;
        }

        public static AvatarSide ParseSide(string position)
        {
            if (string.IsNullOrWhiteSpace(position))
            {
                return AvatarSide.Left;
            }

            return position.Trim().Equals("right", StringComparison.OrdinalIgnoreCase)
                ? AvatarSide.Right
                : AvatarSide.Left;
        }
    }
}
