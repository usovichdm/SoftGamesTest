namespace Features.MagicWords
{
	internal enum DialogueTokenKind
	{
		Text,
		Emoji
	}

	internal readonly struct DialogueToken
	{
		public readonly DialogueTokenKind Kind;
		public readonly string Text;
		public readonly string AtlasId;
		public readonly string Unicode;

		private DialogueToken(DialogueTokenKind kind, string text, string atlasId, string unicode)
		{
			Kind = kind;
			Text = text ?? string.Empty;
			AtlasId = atlasId ?? string.Empty;
			Unicode = unicode ?? string.Empty;
		}

		public static DialogueToken FromText(string value)
		{
			return new DialogueToken(DialogueTokenKind.Text, value, null, null);
		}

		public static DialogueToken FromEmoji(string unicode, string atlasId)
		{
			return new DialogueToken(DialogueTokenKind.Emoji, null, atlasId, unicode);
		}
	}
}
