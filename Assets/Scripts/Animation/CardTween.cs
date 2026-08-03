using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoftGames.Animation
{
    /// <summary>
    /// Runs concurrent card flights without Update() on each card.
    /// </summary>
    public sealed class CardTween : MonoBehaviour
    {
        [SerializeField]
        private float _defaultDuration = 2f;

        [SerializeField]
        private float _arcHeight = 120f;

        public float DefaultDuration
        {
            get { return _defaultDuration; }
        }

        public async UniTask MoveAsync(
            Transform target,
            Vector3 to,
            float duration,
            CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                return;
            }

            var from = target.position;
            duration = Mathf.Max(0.01f, duration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (target == null)
                {
                    return;
                }

                elapsed += Time.deltaTime;
                var t = EasedMotion.EaseInOutCubic(elapsed / duration);
                target.position = EasedMotion.Arc(from, to, t, _arcHeight);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            if (target != null)
            {
                target.position = to;
            }
        }
    }
}
