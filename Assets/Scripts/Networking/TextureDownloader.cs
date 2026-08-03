using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoftGames.Networking
{
    /// <summary>
    /// Downloads and caches textures by URL. Failed URLs are remembered as null.
    /// </summary>
    public sealed class TextureDownloader
    {
        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(16);
        private readonly HashSet<string> _failed = new HashSet<string>();
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

            using (var request = UnityWebRequestTexture.GetTexture(url))
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
                catch (Exception)
                {
                    _failed.Add(url);
                    return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    _failed.Add(url);
                    return null;
                }

                Texture2D texture;
                try
                {
                    texture = DownloadHandlerTexture.GetContent(request);
                }
                catch (Exception)
                {
                    _failed.Add(url);
                    return null;
                }

                if (texture == null)
                {
                    _failed.Add(url);
                    return null;
                }

                _cache[url] = texture;
                return texture;
            }
        }

        /// <summary>
        /// Destroys cached textures and clears failure memory. Call after views that
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
        }
    }
}
