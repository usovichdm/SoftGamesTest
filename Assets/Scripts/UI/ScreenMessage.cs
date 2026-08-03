using System.Threading;
using Cysharp.Threading.Tasks;
using SoftGames.Core;
using TMPro;
using UnityEngine;

namespace SoftGames.UI
{
    /// <summary>
    /// Fades a short status message in/out (used when Ace of Shadows animations go idle).
    /// </summary>
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

        public void HideImmediate()
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
            catch (System.OperationCanceledException)
            {
            }
        }

        private async UniTask FadeAsync(
            float from,
            float to,
            float duration,
            CancellationToken cancellationToken)
        {
            var t = 0f;
            _group.alpha = from;

            while (t < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                t += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _group.alpha = to;
        }

        private void CancelShow()
        {
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
