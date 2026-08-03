using NUnit.Framework;
using SoftGames.Networking;

namespace SoftGames.Tests.EditMode
{
    public sealed class HttpResultTests
    {
        [Test]
        public void Ok_SetsSuccessAndValue_ForStringPayload()
        {
            // Guards against the classic HttpResult<string> constructor overload bug.
            var result = HttpResult<string>.Ok("{\"ok\":true}");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("{\"ok\":true}", result.Value);
            Assert.IsNull(result.Error);
        }

        [Test]
        public void Fail_SetsError_AndClearsValue()
        {
            var result = HttpResult<string>.Fail("nope");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("nope", result.Error);
            Assert.IsNull(result.Value);
        }
    }
}
