using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Features.MagicWords.Tests
{
	public sealed class DialogueResolverTests
	{
		private EmojiCatalogAsset _catalog;

		[SetUp]
		public void SetUp()
		{
			_catalog = ScriptableObject.CreateInstance<EmojiCatalogAsset>();
			_catalog.SetEntries(new[]
			{
				new EmojiCatalogAsset.Entry
				{
					Key = "satisfied",
					Unicode = "😌",
					AtlasId = "1f60a"
				}
			});
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_catalog);
		}

		private DialogueResolver CreateResolver()
		{
			return new DialogueResolver(new EmojiParser(_catalog));
		}

		[Test]
		public void ParseSide_Right_IsCaseInsensitive()
		{
			DialogueResolver.ParseSide("right").Should().Be(AvatarSide.Right);
			DialogueResolver.ParseSide("RIGHT").Should().Be(AvatarSide.Right);
			DialogueResolver.ParseSide("left").Should().Be(AvatarSide.Left);
			DialogueResolver.ParseSide(null).Should().Be(AvatarSide.Left);
			DialogueResolver.ParseSide("  ").Should().Be(AvatarSide.Left);
		}

		[Test]
		public void BuildAvatarMap_FirstNonEmptyUrlWins()
		{
			var map = DialogueResolver.BuildAvatarMap(new[]
			{
				new AvatarEntry { Name = "Sheldon", Url = "", Position = "left" },
				new AvatarEntry { Name = "Sheldon", Url = "http://a", Position = "right" },
				new AvatarEntry { Name = "Sheldon", Url = "http://b", Position = "left" }
			});

			map.Should().HaveCount(1);
			map["Sheldon"].Url.Should().Be("http://a");
			map["Sheldon"].Side.Should().Be(AvatarSide.Right);
		}

		[Test]
		public void BuildLines_BindsAvatarSideAndTokens()
		{
			var response = new MagicWordsResponse
			{
				Dialogue = new[]
				{
					new DialogueEntry { Name = "Sheldon", Text = "Hi {satisfied}" },
					new DialogueEntry { Name = "Unknown", Text = "Hello" }
				},
				Avatars = new[]
				{
					new AvatarEntry
					{
						Name = "Sheldon",
						Url = "http://x",
						Position = "right"
					}
				}
			};

			var lines = CreateResolver().BuildLines(response);

			lines.Should().HaveCount(2);
			lines[0].Speaker.Should().Be("Sheldon");
			lines[0].Side.Should().Be(AvatarSide.Right);
			lines[0].Avatar.Url.Should().Be("http://x");
			lines[0].Tokens.Should().HaveCount(2);

			lines[1].Speaker.Should().Be("Unknown");
			lines[1].Side.Should().Be(AvatarSide.Left);
			lines[1].Avatar.Should().BeNull();
		}

		[Test]
		public void BuildLines_NullResponse_ReturnsEmpty()
		{
			CreateResolver().BuildLines(null).Should().BeEmpty();
		}

		[Test]
		public void BuildLines_SkipsNullEntries_AndTrimsSpeaker()
		{
			var lines = CreateResolver().BuildLines(new MagicWordsResponse
			{
				Dialogue = new[]
				{
					null,
					new DialogueEntry { Name = "  Penny  ", Text = "Hi" }
				},
				Avatars = null
			});

			lines.Should().ContainSingle()
				.Which.Speaker.Should().Be("Penny");
		}
	}
}
