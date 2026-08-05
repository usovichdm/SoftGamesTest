using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Common.Networking
{
	public sealed class TextureDownloader
	{
		private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(16);
		private readonly HashSet<string> _failed = new HashSet<string>();
		private readonly Dictionary<string, UniTaskCompletionSource<Texture2D>> _inFlight =
			new Dictionary<string, UniTaskCompletionSource<Texture2D>>(8);

		private readonly Func<string, UniTask<Texture2D>> _download;
		private readonly int _timeoutSeconds;
		private int _generation;

		public TextureDownloader(int timeoutSeconds = 12)
		{
			_timeoutSeconds = Mathf.Max(1, timeoutSeconds);
			_download = DownloadUncachedAsync;
		}

		internal TextureDownloader(Func<string, UniTask<Texture2D>> download, int timeoutSeconds = 12)
		{
			_download = download ?? throw new ArgumentNullException(nameof(download));
			_timeoutSeconds = Mathf.Max(1, timeoutSeconds);
		}

		internal int Generation => _generation;

		internal bool HasCached(string url)
		{
			return !string.IsNullOrEmpty(url) && _cache.ContainsKey(url);
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
				RunDownloadAsync(url, completion, _generation).Forget();
			}

			return await completion.Task.AttachExternalCancellation(cancellationToken);
		}

		public void Clear()
		{
			_generation++;

			foreach (var pair in _cache)
			{
				DestroyOwned(pair.Value);
			}

			_cache.Clear();
			_failed.Clear();

			foreach (var completion in _inFlight.Values)
			{
				completion.TrySetCanceled();
			}

			_inFlight.Clear();
		}

		private async UniTaskVoid RunDownloadAsync(
			string url,
			UniTaskCompletionSource<Texture2D> completion,
			int generation)
		{
			try
			{
				var texture = await _download(url);
				if (generation != _generation)
				{
					DestroyOwned(texture);
					completion.TrySetCanceled();
					return;
				}

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
				if (generation == _generation)
				{
					_failed.Add(url);
				}

				completion.TrySetException(ex);
			}
			finally
			{
				if (_inFlight.TryGetValue(url, out var current) && ReferenceEquals(current, completion))
				{
					_inFlight.Remove(url);
				}
			}
		}

		private static void DestroyOwned(Texture2D texture)
		{
			if (texture == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(texture);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		private async UniTask<Texture2D> DownloadUncachedAsync(string url)
		{
			using var request = UnityWebRequestTexture.GetTexture(url);
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
