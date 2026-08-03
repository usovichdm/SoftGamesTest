using NUnit.Framework;
using SoftGames.Gameplay.MagicWords;

namespace SoftGames.Tests.EditMode
{
    public sealed class MagicWordsParseTests
    {
        [Test]
        public void ParseJson_ValidPayload_ReturnsDialogueAndAvatars()
        {
            const string json =
                "{\"dialogue\":[{\"name\":\"Sheldon\",\"text\":\"Hi {satisfied}\"}]," +
                "\"avatars\":[{\"name\":\"Sheldon\",\"url\":\"http://x\",\"position\":\"left\"}]}";

            var result = MagicWordsApiClient.ParseJson(json);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Value.dialogue.Length);
            Assert.AreEqual("Sheldon", result.Value.dialogue[0].name);
            Assert.AreEqual(1, result.Value.avatars.Length);
        }

        [Test]
        public void ParseJson_MissingDialogue_Fails()
        {
            var result = MagicWordsApiClient.ParseJson("{\"avatars\":[]}");

            Assert.IsFalse(result.Success);
            StringAssert.Contains("dialogue", result.Error);
        }

        [Test]
        public void ParseJson_MissingAvatars_DefaultsToEmptyArray()
        {
            const string json = "{\"dialogue\":[{\"name\":\"Penny\",\"text\":\"Hi\"}]}";

            var result = MagicWordsApiClient.ParseJson(json);

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Value.avatars);
            Assert.AreEqual(0, result.Value.avatars.Length);
        }

        [Test]
        public void ParseJson_EmptyBody_Fails()
        {
            var result = MagicWordsApiClient.ParseJson("   ");
            Assert.IsFalse(result.Success);
        }
    }
}
