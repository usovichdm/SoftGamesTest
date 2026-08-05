using Common.Networking;
using Cysharp.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Common.Networking.Tests
{
	public sealed class TextureDownloaderTests
	{
		[Test]
		public async Task Clear_CancelsInFlightWaiters_AndDropsStaleResult()
		{
			var started = new UniTaskCompletionSource();
			var gate = new UniTaskCompletionSource();
			var texture = new Texture2D(2, 2);
			var downloader = new TextureDownloader(async _ =>
			{
				started.TrySetResult();
				await gate.Task;
				return texture;
			});

			var download = downloader.DownloadAsync("https://example.test/avatar.png").AsTask();
			await started.Task;

			downloader.Clear();
			downloader.Generation.Should().Be(1);

			gate.TrySetResult();

			Func<Task> act = async () => await download;
			await act.Should().ThrowAsync<OperationCanceledException>();

			downloader.HasCached("https://example.test/avatar.png").Should().BeFalse();

			if (texture)
			{
				UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		[Test]
		public async Task DownloadAsync_EmptyUrl_ReturnsNull()
		{
			var downloader = new TextureDownloader(_ => UniTask.FromResult<Texture2D>(null));
			var result = await downloader.DownloadAsync("  ");
			result.Should().BeNull();
		}

		[Test]
		public async Task Clear_ThenDownload_UsesFreshGeneration()
		{
			var created = new List<Texture2D>();
			var downloader = new TextureDownloader(_ =>
			{
				var texture = new Texture2D(2, 2);
				created.Add(texture);
				return UniTask.FromResult(texture);
			});

			var first = await downloader.DownloadAsync("https://example.test/a.png");
			first.Should().NotBeNull();
			downloader.HasCached("https://example.test/a.png").Should().BeTrue();

			downloader.Clear();
			downloader.HasCached("https://example.test/a.png").Should().BeFalse();

			var second = await downloader.DownloadAsync("https://example.test/a.png");
			second.Should().NotBeNull();
			second.Should().NotBeSameAs(first);
			created.Should().HaveCount(2);

			for (var i = 0; i < created.Count; i++)
			{
				if (created[i])
				{
					UnityEngine.Object.DestroyImmediate(created[i]);
				}
			}
		}
	}
}
