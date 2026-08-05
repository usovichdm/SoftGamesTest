using System.Collections.Generic;
using Newtonsoft.Json;

namespace Features.MagicWords
{
	internal enum AvatarSide
	{
		Left,
		Right
	}

	internal sealed class MagicWordsResponse
	{
		[JsonProperty("dialogue")]
		public DialogueEntry[] Dialogue { get; set; }

		[JsonProperty("avatars")]
		public AvatarEntry[] Avatars { get; set; }
	}

	internal sealed class DialogueEntry
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}

	internal sealed class AvatarEntry
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("position")]
		public string Position { get; set; }
	}

	internal sealed class ResolvedAvatar
	{
		public string Url;
		public AvatarSide Side;
	}

	internal sealed class ResolvedDialogueLine
	{
		public string Speaker;
		public IReadOnlyList<DialogueToken> Tokens;
		public ResolvedAvatar Avatar;
		public AvatarSide Side;
	}
}
