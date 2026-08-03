using NUnit.Framework;
using SoftGames.Gameplay.MagicWords;

namespace SoftGames.Tests.EditMode
{
    public sealed class DialogueResolverTests
    {
        [Test]
        public void ParseSide_Right_IsCaseInsensitive()
        {
            Assert.AreEqual(AvatarSide.Right, DialogueResolver.ParseSide("right"));
            Assert.AreEqual(AvatarSide.Right, DialogueResolver.ParseSide("RIGHT"));
            Assert.AreEqual(AvatarSide.Left, DialogueResolver.ParseSide("left"));
            Assert.AreEqual(AvatarSide.Left, DialogueResolver.ParseSide(null));
            Assert.AreEqual(AvatarSide.Left, DialogueResolver.ParseSide("  "));
        }

        [Test]
        public void BuildAvatarMap_FirstNonEmptyUrlWins()
        {
            var map = DialogueResolver.BuildAvatarMap(new[]
            {
                new AvatarEntry { name = "Sheldon", url = "", position = "left" },
                new AvatarEntry { name = "Sheldon", url = "http://a", position = "right" },
                new AvatarEntry { name = "Sheldon", url = "http://b", position = "left" }
            });

            Assert.AreEqual(1, map.Count);
            Assert.AreEqual("http://a", map["Sheldon"].Url);
            Assert.AreEqual(AvatarSide.Right, map["Sheldon"].Side);
        }

        [Test]
        public void BuildLines_BindsAvatarSideAndTokens()
        {
            var resolver = new DialogueResolver();
            var response = new MagicWordsResponse
            {
                dialogue = new[]
                {
                    new DialogueEntry { name = "Sheldon", text = "Hi {satisfied}" },
                    new DialogueEntry { name = "Unknown", text = "Hello" }
                },
                avatars = new[]
                {
                    new AvatarEntry
                    {
                        name = "Sheldon",
                        url = "http://x",
                        position = "right"
                    }
                }
            };

            var lines = resolver.BuildLines(response);

            Assert.AreEqual(2, lines.Count);
            Assert.AreEqual("Sheldon", lines[0].Speaker);
            Assert.AreEqual(AvatarSide.Right, lines[0].Side);
            Assert.AreEqual("http://x", lines[0].Avatar.Url);
            Assert.AreEqual(2, lines[0].Tokens.Count);

            Assert.AreEqual("Unknown", lines[1].Speaker);
            Assert.AreEqual(AvatarSide.Left, lines[1].Side);
            Assert.IsNull(lines[1].Avatar);
        }

        [Test]
        public void BuildLines_NullResponse_ReturnsEmpty()
        {
            var lines = new DialogueResolver().BuildLines(null);
            Assert.AreEqual(0, lines.Count);
        }

        [Test]
        public void BuildLines_SkipsNullEntries_AndTrimsSpeaker()
        {
            var lines = new DialogueResolver().BuildLines(new MagicWordsResponse
            {
                dialogue = new[]
                {
                    null,
                    new DialogueEntry { name = "  Penny  ", text = "Hi" }
                },
                avatars = null
            });

            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual("Penny", lines[0].Speaker);
        }
    }
}
