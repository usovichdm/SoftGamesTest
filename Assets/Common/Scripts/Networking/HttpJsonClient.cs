using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Common.Networking
{
	public sealed class HttpJsonClient
	{
		private readonly int _timeoutSeconds;

		public HttpJsonClient(int timeoutSeconds = 12)
		{
			_timeoutSeconds = Mathf.Max(1, timeoutSeconds);
		}

		public async UniTask<HttpResult<string>> GetRawJsonAsync(
			string url,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return HttpResult<string>.Fail(NetworkTexts.UrlEmpty);
			}

			using var request = UnityWebRequest.Get(url);
			request.timeout = _timeoutSeconds;

			try
			{
				await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				return HttpResult<string>.Fail(DescribeFailure(ex.Message, request));
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				return HttpResult<string>.Fail(DescribeFailure(request.error, request));
			}

			var body = request.downloadHandler?.text;
			if (string.IsNullOrWhiteSpace(body))
			{
				return HttpResult<string>.Fail(NetworkTexts.EmptyResponseBody);
			}

			return HttpResult<string>.Ok(body);
		}

		public async UniTask<HttpResult<T>> GetJsonAsync<T>(
			string url,
			CancellationToken cancellationToken = default)
		{
			var raw = await GetRawJsonAsync(url, cancellationToken);
			if (!raw.Success)
			{
				return HttpResult<T>.Fail(raw.Error);
			}

			return DeserializeJson<T>(raw.Value);
		}

		public static HttpResult<T> DeserializeJson<T>(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return HttpResult<T>.Fail(NetworkTexts.EmptyResponseBody);
			}

			T parsed;
			try
			{
				parsed = JsonConvert.DeserializeObject<T>(json);
			}
			catch (Exception ex)
			{
				return HttpResult<T>.Fail(NetworkTexts.MalformedJsonPrefix + ex.Message);
			}

			if (parsed == null)
			{
				return HttpResult<T>.Fail(NetworkTexts.MalformedJsonNull);
			}

			return HttpResult<T>.Ok(parsed);
		}

		private static string DescribeFailure(string detail, UnityWebRequest request)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				return NetworkTexts.NoInternet;
			}

			var raw = detail ?? string.Empty;

			if (request is { result: UnityWebRequest.Result.ConnectionError })
			{
				if (raw.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0
				    || raw.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return NetworkTexts.TimedOut;
				}

				return NetworkTexts.CouldNotReachServer;
			}

			if (request is { responseCode: >= 500 })
			{
				return string.Format(NetworkTexts.ServerErrorFormat, request.responseCode);
			}

			if (request is { responseCode: >= 400 })
			{
				return string.Format(NetworkTexts.RequestFailedFormat, request.responseCode);
			}

			if (string.IsNullOrWhiteSpace(raw))
			{
				return NetworkTexts.NetworkRequestFailed;
			}

			return NetworkTexts.NetworkErrorPrefix + raw;
		}
	}
}
