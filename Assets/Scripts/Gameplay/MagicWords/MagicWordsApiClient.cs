using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoftGames.Networking;
using UnityEngine;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Fetches and parses Magic Words JSON. Keeps transport/parsing out of UI code.
    /// </summary>
    public sealed class MagicWordsApiClient
    {
        public const string DefaultEndpoint =
            "https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords";

        private readonly HttpJsonClient _http;
        private readonly string _endpoint;

        public MagicWordsApiClient(HttpJsonClient http, string endpoint = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
        }

        public async UniTask<HttpResult<MagicWordsResponse>> FetchAsync(
            CancellationToken cancellationToken = default)
        {
            var result = await _http.GetJsonAsync(_endpoint, cancellationToken);
            if (!result.Success)
            {
                return HttpResult<MagicWordsResponse>.Fail(result.Error);
            }

            return ParseJson(result.Value);
        }

        /// <summary>
        /// Pure JSON → model conversion used by runtime and Edit Mode tests.
        /// </summary>
        public static HttpResult<MagicWordsResponse> ParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return HttpResult<MagicWordsResponse>.Fail("Response body is empty.");
            }

            MagicWordsResponse parsed;
            try
            {
                parsed = JsonUtility.FromJson<MagicWordsResponse>(json);
            }
            catch (Exception ex)
            {
                return HttpResult<MagicWordsResponse>.Fail($"Malformed JSON: {ex.Message}");
            }

            if (parsed == null)
            {
                return HttpResult<MagicWordsResponse>.Fail("Malformed JSON: parser returned null.");
            }

            if (parsed.dialogue == null)
            {
                return HttpResult<MagicWordsResponse>.Fail("Response missing 'dialogue' field.");
            }

            if (parsed.avatars == null)
            {
                parsed.avatars = Array.Empty<AvatarEntry>();
            }

            return HttpResult<MagicWordsResponse>.Ok(parsed);
        }
    }
}
