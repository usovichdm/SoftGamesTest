using Features.MagicWords;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Features.MagicWords.Tests
{
	public sealed class EmojiParserTests
	{
		private EmojiCatalogAsset _catalog;
		private EmojiParser _parser;

		[SetUp]
		public void SetUp()
		{
			_catalog = ScriptableObject.CreateInstance<EmojiCatalogAsset>();
			_catalog.SetEntries(new[]
			{
				new EmojiCatalogAsset.Entry
				{
					Key = "satisfied",
					Unicode = "😊",
					AtlasId = "Smiling face with smiling eyes"
				}
			});
			_parser = new EmojiParser(_catalog);
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_catalog);
		}

		[Test]
		public void Parse_PlainText_ReturnsSingleTextToken()
		{
			var tokens = _parser.Parse("Hello there");

			tokens.Should().ContainSingle();
			tokens[0].Kind.Should().Be(DialogueTokenKind.Text);
			tokens[0].Text.Should().Be("Hello there");
		}

		[Test]
		public void Parse_KnownEmojiToken_InsertsEmojiToken()
		{
			var tokens = _parser.Parse("I admit {satisfied} this.");

			tokens.Should().HaveCount(3);
			tokens[0].Kind.Should().Be(DialogueTokenKind.Text);
			tokens[1].Kind.Should().Be(DialogueTokenKind.Emoji);
			tokens[2].Kind.Should().Be(DialogueTokenKind.Text);

			tokens[1].Unicode.Should().Be("😊");
			tokens[1].AtlasId.Should().Be("Smiling face with smiling eyes");
		}

		[Test]
		public void Parse_UnknownToken_KeptAsText()
		{
			var tokens = _parser.Parse("Hello {not_a_real_token}");

			tokens.Should().HaveCount(2);
			tokens[1].Kind.Should().Be(DialogueTokenKind.Text);
			tokens[1].Text.Should().Be("{not_a_real_token}");
		}

		[Test]
		public void Parse_NullOrEmpty_ReturnsEmptyList()
		{
			_parser.Parse(null).Should().BeEmpty();
			_parser.Parse(string.Empty).Should().BeEmpty();
		}
	}
}
