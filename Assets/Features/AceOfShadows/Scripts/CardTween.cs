using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

namespace Features.AceOfShadows
{
	internal sealed class CardTween : MonoBehaviour
	{
		[SerializeField]
		private float _defaultDuration = 2f;

		[SerializeField]
		private float _arcHeight = 140f;

		[SerializeField]
		private float _peakScale = 1.1f;

		[SerializeField]
		private float _tiltDegrees = 12f;

		public float DefaultDuration => _defaultDuration;

		private void Awake()
		{
			DOTween.Init(recycleAllByDefault: false, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);
			DOTween.defaultUpdateType = UpdateType.Normal;
			DOTween.defaultTimeScaleIndependent = false;
		}

		public UniTask MoveAsync(
			Transform target,
			Vector3 to,
			float duration,
			CancellationToken cancellationToken = default)
		{
			if (target == null)
			{
				return UniTask.FromCanceled(cancellationToken);
			}

			cancellationToken.ThrowIfCancellationRequested();

			duration = Mathf.Max(0.01f, duration);

			var from = target.position;
			var mid = Vector3.LerpUnclamped(from, to, 0.5f);
			mid.y += _arcHeight;

			var baseScale = target.localScale;
			var peakScale = baseScale * _peakScale;
			var baseEuler = target.localEulerAngles;
			var peakEuler = baseEuler + new Vector3(0f, 0f, _tiltDegrees);

			target.DOKill(complete: false);

			var tcs = new UniTaskCompletionSource();
			var completed = false;

			var sequence = DOTween.Sequence()
				.SetTarget(target)
				.SetLink(target.gameObject, LinkBehaviour.KillOnDestroy)
				.SetUpdate(UpdateType.Normal, isIndependentUpdate: false);

			sequence.Append(
				target
					.DOPath(new[] { from, mid, to }, duration, PathType.CatmullRom)
					.SetEase(Ease.InOutCubic)
					.SetOptions(closePath: false));

			sequence.Join(
				target
					.DOScale(peakScale, duration * 0.45f)
					.SetEase(Ease.OutQuad));
			sequence.Insert(
				duration * 0.45f,
				target
					.DOScale(baseScale, duration * 0.55f)
					.SetEase(Ease.InOutQuad));

			sequence.Join(
				target
					.DOLocalRotate(peakEuler, duration * 0.45f)
					.SetEase(Ease.OutSine));
			sequence.Insert(
				duration * 0.45f,
				target
					.DOLocalRotate(baseEuler, duration * 0.55f)
					.SetEase(Ease.InSine));

			sequence.OnComplete(() =>
			{
				completed = true;
				if (target != null)
				{
					target.position = to;
					target.localScale = baseScale;
					target.localEulerAngles = baseEuler;
				}

				tcs.TrySetResult();
			});

			sequence.OnKill(() =>
			{
				if (!completed)
				{
					tcs.TrySetCanceled(cancellationToken);
				}
			});

			var registration = cancellationToken.Register(() =>
			{
				if (sequence.IsActive())
				{
					sequence.Kill(complete: false);
				}
			});

			return AwaitAndDispose(tcs.Task, registration);
		}

		private static async UniTask AwaitAndDispose(UniTask task, CancellationTokenRegistration registration)
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
	}
}
