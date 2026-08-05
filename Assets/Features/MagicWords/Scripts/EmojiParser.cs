using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Features.MagicWords
{
	internal sealed class EmojiParser
	{
		private static readonly Regex TokenRegex = new Regex(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

		private readonly EmojiCatalogAsset _catalog;

		public EmojiParser(EmojiCatalogAsset catalog)
		{
			if (catalog == null)
			{
				throw new System.ArgumentNullException(nameof(catalog));
			}

			_catalog = catalog;
		}

		public List<DialogueToken> Parse(string text)
		{
			var tokens = new List<DialogueToken>(8);
			if (string.IsNullOrEmpty(text))
			{
				return tokens;
			}

			var matches = TokenRegex.Matches(text);
			if (matches.Count == 0)
			{
				tokens.Add(DialogueToken.FromText(text));
				return tokens;
			}

			var cursor = 0;
			for (var i = 0; i < matches.Count; i++)
			{
				var match = matches[i];
				if (match.Index > cursor)
				{
					tokens.Add(DialogueToken.FromText(text.Substring(cursor, match.Index - cursor)));
				}

				var key = match.Groups[1].Value;
				if (_catalog.TryGet(key, out var entry))
				{
					tokens.Add(DialogueToken.FromEmoji(entry.Unicode, entry.AtlasId));
				}
				else
				{
					tokens.Add(DialogueToken.FromText(match.Value));
				}

				cursor = match.Index + match.Length;
			}

			if (cursor < text.Length)
			{
				tokens.Add(DialogueToken.FromText(text.Substring(cursor)));
			}

			return tokens;
		}
	}
}
