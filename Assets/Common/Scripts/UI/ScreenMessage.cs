using Common.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;

namespace Common.UI
{
	public sealed class ScreenMessage : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup _group;

		[SerializeField]
		private TMP_Text _label;

		[SerializeField]
		private float _fadeSeconds = 0.35f;

		[SerializeField]
		private float _holdSeconds = 1.6f;

		private CancellationTokenSource _showCts;

		private void Awake()
		{
			_label.color = AppColors.TextPrimary;
			HideImmediate();
		}

		private void OnDestroy()
		{
			CancelShow();
		}

		public void Show(string message)
		{
			_label.text = message;
			CancelShow();
			_showCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			ShowAsync(_showCts.Token).Forget();
		}

		private void HideImmediate()
		{
			CancelShow();
			_group.alpha = 0f;
			_group.blocksRaycasts = false;
			_group.interactable = false;
		}

		private async UniTaskVoid ShowAsync(CancellationToken cancellationToken)
		{
			try
			{
				_group.blocksRaycasts = false;
				await FadeAsync(0f, 1f, _fadeSeconds, cancellationToken);
				await UniTask.Delay(
					System.TimeSpan.FromSeconds(_holdSeconds),
					DelayType.Realtime,
					cancellationToken: cancellationToken);
				await FadeAsync(1f, 0f, _fadeSeconds, cancellationToken);
			}
			catch (System.OperationCanceledException) { }
		}

		private UniTask FadeAsync(
			float from,
			float to,
			float duration,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			_group.alpha = from;

			var tcs = new UniTaskCompletionSource();
			var completed = false;

			var tween = DOTween
				.To(() => _group.alpha, x => _group.alpha = x, to, duration)
				.SetTarget(_group)
				.SetUpdate(isIndependentUpdate: true)
				.SetEase(Ease.Linear)
				.SetLink(gameObject, LinkBehaviour.KillOnDestroy)
				.OnComplete(() =>
				{
					completed = true;
					tcs.TrySetResult();
				})
				.OnKill(() =>
				{
					if (!completed)
					{
						tcs.TrySetCanceled(cancellationToken);
					}
				});

			var registration = cancellationToken.Register(() =>
			{
				if (tween.IsActive())
				{
					tween.Kill(complete: false);
				}
			});

			return AwaitAndDispose(tcs.Task, registration);
		}

		private static async UniTask AwaitAndDispose(
			UniTask task,
			CancellationTokenRegistration registration)
		{
			try
			{
				await task;
			}
			finally
			{
				await registration.DisposeAsync();
			}
		}

		private void CancelShow()
		{
			_group.DOKill();

			if (_showCts == null)
			{
				return;
			}

			_showCts.Cancel();
			_showCts.Dispose();
			_showCts = null;
		}
	}
}
