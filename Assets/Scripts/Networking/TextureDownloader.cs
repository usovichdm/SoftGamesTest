using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoftGames.Networking
{
    /// <summary>
    /// Downloads and caches textures by URL. Concurrent requests for the same URL share one download.
    /// Failed URLs are remembered as null until <see cref="Clear"/>.
    /// </summary>
    public sealed class TextureDownloader
    {
        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(16);
        private readonly HashSet<string> _failed = new HashSet<string>();
        private readonly Dictionary<string, UniTaskCompletionSource<Texture2D>> _inFlight =
            new Dictionary<string, UniTaskCompletionSource<Texture2D>>(8);

        private readonly int _timeoutSeconds;

        public TextureDownloader(int timeoutSeconds = 12)
        {
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public async UniTask<Texture2D> DownloadAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (_failed.Contains(url))
            {
                return null;
            }

            if (_cache.TryGetValue(url, out var cached))
            {
                return cached;
            }

            if (!_inFlight.TryGetValue(url, out var completion))
            {
                completion = new UniTaskCompletionSource<Texture2D>();
                _inFlight[url] = completion;
                RunDownloadAsync(url, completion).Forget();
            }

            return await completion.Task.AttachExternalCancellation(cancellationToken);
        }

        /// <summary>
        /// Destroys cached textures and clears failure / in-flight memory. Call after views that
        /// referenced those textures have released their sprites.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _cache)
            {
                if (pair.Value != null)
                {
                    UnityEngine.Object.Destroy(pair.Value);
                }
            }

            _cache.Clear();
            _failed.Clear();
            _inFlight.Clear();
        }

        private async UniTaskVoid RunDownloadAsync(
            string url,
            UniTaskCompletionSource<Texture2D> completion)
        {
            try
            {
                var texture = await DownloadUncachedAsync(url);
                if (texture == null)
                {
                    _failed.Add(url);
                }
                else
                {
                    _cache[url] = texture;
                }

                completion.TrySetResult(texture);
            }
            catch (Exception ex)
            {
                _failed.Add(url);
                completion.TrySetException(ex);
            }
            finally
            {
                _inFlight.Remove(url);
            }
        }

        private async UniTask<Texture2D> DownloadUncachedAsync(string url)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                request.timeout = _timeoutSeconds;

                try
                {
                    await request.SendWebRequest();
                }
                catch (Exception)
                {
                    return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return null;
                }

                try
                {
                    return DownloadHandlerTexture.GetContent(request);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
