using Common.Networking;
using System;

namespace Features.MagicWords
{
	internal static class MagicWordsParse
	{
		public const string DefaultEndpoint =
			"https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords";

		public static HttpResult<MagicWordsResponse> ParseJson(string json)
		{
			var result = HttpJsonClient.DeserializeJson<MagicWordsResponse>(json);
			if (!result.Success)
			{
				return result;
			}

			return Validate(result.Value);
		}

		public static HttpResult<MagicWordsResponse> Validate(MagicWordsResponse parsed)
		{
			if (parsed.Dialogue == null)
			{
				return HttpResult<MagicWordsResponse>.Fail(MagicWordsTexts.MissingDialogueField);
			}

			if (parsed.Avatars == null)
			{
				parsed.Avatars = Array.Empty<AvatarEntry>();
			}

			return HttpResult<MagicWordsResponse>.Ok(parsed);
		}
	}
}
