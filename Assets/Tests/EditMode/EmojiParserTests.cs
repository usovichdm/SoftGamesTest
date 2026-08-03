using NUnit.Framework;
using SoftGames.Gameplay.MagicWords;

namespace SoftGames.Tests.EditMode
{
    public sealed class EmojiParserTests
    {
        private readonly EmojiParser _parser = new EmojiParser();

        [Test]
        public void Parse_PlainText_ReturnsSingleTextToken()
        {
            var tokens = _parser.Parse(new DialogueMessage("Hello there"));

            Assert.AreEqual(1, tokens.Count);
            Assert.IsInstanceOf<TextToken>(tokens[0]);
            Assert.AreEqual("Hello there", ((TextToken)tokens[0]).Value);
        }

        [Test]
        public void Parse_KnownEmojiToken_InsertsEmojiToken()
        {
            var tokens = _parser.Parse(new DialogueMessage("I admit {satisfied} this."));

            Assert.AreEqual(3, tokens.Count);
            Assert.IsInstanceOf<TextToken>(tokens[0]);
            Assert.IsInstanceOf<EmojiToken>(tokens[1]);
            Assert.IsInstanceOf<TextToken>(tokens[2]);

            var emoji = (EmojiToken)tokens[1];
            Assert.AreEqual("satisfied", emoji.Key);
            Assert.AreEqual("😌", emoji.Unicode);
            Assert.AreEqual("1f60a", emoji.AtlasId);
        }

        [Test]
        public void Parse_UnknownToken_KeptAsText()
        {
            var tokens = _parser.Parse(new DialogueMessage("Hello {not_a_real_token}"));

            Assert.AreEqual(2, tokens.Count);
            Assert.IsInstanceOf<TextToken>(tokens[1]);
            Assert.AreEqual("{not_a_real_token}", ((TextToken)tokens[1]).Value);
        }

        [Test]
        public void Parse_NullMessage_ReturnsEmptyList()
        {
            var tokens = _parser.Parse(null);
            Assert.AreEqual(0, tokens.Count);
        }
    }
}
