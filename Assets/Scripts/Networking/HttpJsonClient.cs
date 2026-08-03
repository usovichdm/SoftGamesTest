using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoftGames.Networking
{
    public readonly struct HttpResult<T>
    {
        public readonly bool Success;
        public readonly T Value;
        public readonly string Error;

        private HttpResult(bool success, T value, string error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static HttpResult<T> Ok(T value)
        {
            return new HttpResult<T>(true, value, null);
        }

        public static HttpResult<T> Fail(string error)
        {
            return new HttpResult<T>(false, default, error ?? "Unknown error");
        }
    }

    /// <summary>
    /// Minimal UnityWebRequest JSON GET helper. Presentation stays out of transport details.
    /// </summary>
    public sealed class HttpJsonClient
    {
        private readonly int _timeoutSeconds;

        public HttpJsonClient(int timeoutSeconds = 12)
        {
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public async UniTask<HttpResult<string>> GetJsonAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return HttpResult<string>.Fail("URL is empty.");
            }

            using (var request = UnityWebRequest.Get(url))
            {
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
                    return HttpResult<string>.Fail($"Request failed: {ex.Message}");
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return HttpResult<string>.Fail($"Request failed: {request.error}");
                }

                var body = request.downloadHandler != null ? request.downloadHandler.text : null;
                if (string.IsNullOrWhiteSpace(body))
                {
                    return HttpResult<string>.Fail("Response body is empty.");
                }

                return HttpResult<string>.Ok(body);
            }
        }
    }
}
