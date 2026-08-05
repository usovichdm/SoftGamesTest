using Common.Networking;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MagicWords
{
	internal sealed class DialogueFeed
	{
		private const int SpawnYieldMask = 3;

		private readonly DialogueLineView _linePrefab;
		private readonly RectTransform _contentRoot;
		private readonly ScrollRect _scrollRect;
		private readonly LayoutGroup _contentLayout;
		private readonly TextureDownloader _textures = new TextureDownloader();
		private readonly List<DialogueLineView> _spawned = new List<DialogueLineView>(32);
		private int _bindGeneration;

		public DialogueFeed(
			DialogueLineView linePrefab,
			RectTransform contentRoot,
			ScrollRect scrollRect)
		{
			_linePrefab = linePrefab;
			_contentRoot = contentRoot;
			_scrollRect = scrollRect;
			_contentLayout = contentRoot.GetComponent<LayoutGroup>();
		}

		public async UniTask BindAsync(
			IReadOnlyList<ResolvedDialogueLine> lines,
			CancellationToken cancellationToken)
		{
			Clear();
			var generation = _bindGeneration;

			if (_contentLayout != null)
			{
				_contentLayout.enabled = false;
			}

			var avatarJobs = new List<(AvatarView view, string speaker, string url)>(lines.Count);

			try
			{
				for (var i = 0; i < lines.Count; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (generation != _bindGeneration)
					{
						return;
					}

					var line = lines[i];
					var view = Object.Instantiate(_linePrefab, _contentRoot);
					if (generation != _bindGeneration)
					{
						Object.Destroy(view.gameObject);
						return;
					}

					view.Bind(line);
					_spawned.Add(view);

					var url = line.Avatar != null ? line.Avatar.Url : null;
					if (!string.IsNullOrEmpty(url))
					{
						avatarJobs.Add((view.Avatar, line.Speaker, url));
					}

					if ((i & SpawnYieldMask) == SpawnYieldMask)
					{
						await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
					}
				}

				if (generation != _bindGeneration)
				{
					return;
				}

				if (_contentLayout != null)
				{
					_contentLayout.enabled = true;
				}

				Canvas.ForceUpdateCanvases();
				for (var i = 0; i < _spawned.Count; i++)
				{
					_spawned[i].RefreshLayout();
				}

				_scrollRect.normalizedPosition = new Vector2(0f, 1f);

				for (var i = 0; i < avatarJobs.Count; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (generation != _bindGeneration)
					{
						return;
					}

					var job = avatarJobs[i];
					await ApplyAvatarAsync(job.view, job.speaker, job.url, generation, cancellationToken);
					await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
				}
			}
			finally
			{
				if (generation == _bindGeneration && _contentLayout != null && !_contentLayout.enabled)
				{
					_contentLayout.enabled = true;
				}
			}
		}

		public void Clear()
		{
			_bindGeneration++;

			for (var i = 0; i < _spawned.Count; i++)
			{
				var line = _spawned[i];
				if (line == null)
				{
					continue;
				}

				if (line.Avatar != null)
				{
					line.Avatar.ReleaseResources();
				}

				Object.Destroy(line.gameObject);
			}

			_spawned.Clear();
			_textures.Clear();
		}

		private async UniTask ApplyAvatarAsync(
			AvatarView avatarView,
			string speaker,
			string url,
			int generation,
			CancellationToken cancellationToken)
		{
			try
			{
				var texture = await _textures.DownloadAsync(url, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();

				if (generation != _bindGeneration || avatarView == null)
				{
					return;
				}

				if (texture == null || !texture)
				{
					avatarView.ShowPlaceholder(speaker);
					return;
				}

				avatarView.ShowTexture(texture, speaker);
			}
			catch (System.OperationCanceledException) { }
		}
	}
}
