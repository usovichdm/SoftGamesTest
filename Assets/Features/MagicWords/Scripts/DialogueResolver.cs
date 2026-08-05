using System;
using System.Collections.Generic;

namespace Features.MagicWords
{
	internal sealed class DialogueResolver
	{
		private readonly EmojiParser _parser;

		public DialogueResolver(EmojiParser parser)
		{
			_parser = parser ?? throw new ArgumentNullException(nameof(parser));
		}

		public List<ResolvedDialogueLine> BuildLines(MagicWordsResponse response)
		{
			var avatars = BuildAvatarMap(response?.Avatars);
			var source = response != null && response.Dialogue != null
				? response.Dialogue
				: Array.Empty<DialogueEntry>();

			var lines = new List<ResolvedDialogueLine>(source.Length);
			for (var i = 0; i < source.Length; i++)
			{
				var entry = source[i];
				if (entry == null)
				{
					continue;
				}

				var speaker = string.IsNullOrWhiteSpace(entry.Name)
					? MagicWordsTexts.UnknownSpeaker
					: entry.Name.Trim();
				avatars.TryGetValue(speaker, out var avatar);

				lines.Add(new ResolvedDialogueLine
				{
					Speaker = speaker,
					Tokens = _parser.Parse(entry.Text),
					Avatar = avatar,
					Side = avatar?.Side ?? AvatarSide.Left
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
				if (entry == null || string.IsNullOrWhiteSpace(entry.Name))
				{
					continue;
				}

				var name = entry.Name.Trim();
				var url = entry.Url != null ? entry.Url.Trim() : string.Empty;
				var side = ParseSide(entry.Position);

				if (map.TryGetValue(name, out var existing))
				{
					if (string.IsNullOrEmpty(existing.Url) && !string.IsNullOrEmpty(url))
					{
						existing.Url = url;
						existing.Side = side;
					}

					continue;
				}

				map[name] = new ResolvedAvatar
				{
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

			return position.Trim().Equals(MagicWordsTexts.SideRight, StringComparison.OrdinalIgnoreCase)
				? AvatarSide.Right
				: AvatarSide.Left;
		}
	}
}
