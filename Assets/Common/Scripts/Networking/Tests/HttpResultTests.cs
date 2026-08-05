using Common.Networking;
using FluentAssertions;
using NUnit.Framework;

namespace Common.Networking.Tests
{
	public sealed class HttpResultTests
	{
		[Test]
		public void Ok_SetsSuccessAndValue()
		{
			var result = HttpResult<int>.Ok(42);

			result.Success.Should().BeTrue();
			result.Value.Should().Be(42);
			result.Error.Should().BeNull();
		}

		[Test]
		public void Fail_SetsError_AndDefaultValue()
		{
			var result = HttpResult<string>.Fail("boom");

			result.Success.Should().BeFalse();
			result.Value.Should().BeNull();
			result.Error.Should().Be("boom");
		}

		[Test]
		public void Fail_NullError_UsesUnknownError()
		{
			var result = HttpResult<object>.Fail(null);

			result.Success.Should().BeFalse();
			result.Error.Should().Be(NetworkTexts.UnknownError);
		}

		[Test]
		public void DeserializeJson_Empty_Fails()
		{
			var result = HttpJsonClient.DeserializeJson<DummyDto>("  ");

			result.Success.Should().BeFalse();
			result.Error.Should().Be(NetworkTexts.EmptyResponseBody);
		}

		[Test]
		public void DeserializeJson_Malformed_Fails()
		{
			var result = HttpJsonClient.DeserializeJson<DummyDto>("{not-json");

			result.Success.Should().BeFalse();
			result.Error.Should().StartWith(NetworkTexts.MalformedJsonPrefix);
		}

		[Test]
		public void DeserializeJson_ValidPayload_ReturnsValue()
		{
			var result = HttpJsonClient.DeserializeJson<DummyDto>("{\"Name\":\"Ada\"}");

			result.Success.Should().BeTrue();
			result.Value.Name.Should().Be("Ada");
		}

		private sealed class DummyDto
		{
			public string Name { get; set; }
		}
	}
}
