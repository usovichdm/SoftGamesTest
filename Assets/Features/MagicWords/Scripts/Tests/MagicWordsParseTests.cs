using Features.MagicWords;
using FluentAssertions;
using NUnit.Framework;

namespace Features.MagicWords.Tests
{
	public sealed class MagicWordsParseTests
	{
		[Test]
		public void ParseJson_ValidPayload_ReturnsDialogueAndAvatars()
		{
			const string json =
				"{\"dialogue\":[{\"name\":\"Sheldon\",\"text\":\"Hi {satisfied}\"}]," +
				"\"avatars\":[{\"name\":\"Sheldon\",\"url\":\"http://x\",\"position\":\"left\"}]}";

			var result = MagicWordsParse.ParseJson(json);

			result.Success.Should().BeTrue();
			result.Value.Dialogue.Should().ContainSingle()
				.Which.Name.Should().Be("Sheldon");
			result.Value.Avatars.Should().HaveCount(1);
			result.Value.Avatars[0].Url.Should().Be("http://x");
		}

		[Test]
		public void ParseJson_MissingDialogue_Fails()
		{
			var result = MagicWordsParse.ParseJson("{\"avatars\":[]}");

			result.Success.Should().BeFalse();
			result.Error.Should().Contain("dialogue");
		}

		[Test]
		public void ParseJson_MissingAvatars_DefaultsToEmptyArray()
		{
			const string json = "{\"dialogue\":[{\"name\":\"Penny\",\"text\":\"Hi\"}]}";

			var result = MagicWordsParse.ParseJson(json);

			result.Success.Should().BeTrue();
			result.Value.Avatars.Should().NotBeNull().And.BeEmpty();
		}

		[Test]
		public void ParseJson_EmptyBody_Fails()
		{
			MagicWordsParse.ParseJson("   ").Success.Should().BeFalse();
		}
	}
}
