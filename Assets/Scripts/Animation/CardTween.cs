using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoftGames.Animation
{
    /// <summary>
    /// Concurrent card flights driven by a single Update (no per-card async Yield).
    /// </summary>
    public sealed class CardTween : MonoBehaviour
    {
        private struct Flight
        {
            public Transform Target;
            public Vector3 From;
            public Vector3 To;
            public float Duration;
            public float Elapsed;
            public UniTaskCompletionSource Completion;
            public CancellationToken Cancellation;
        }

        [SerializeField]
        private float _defaultDuration = 2f;

        [SerializeField]
        private float _arcHeight = 120f;

        private readonly List<Flight> _flights = new List<Flight>(8);

        public float DefaultDuration
        {
            get { return _defaultDuration; }
        }

        public UniTask MoveAsync(
            Transform target,
            Vector3 to,
            float duration,
            CancellationToken cancellationToken = default)
        {
            if (target == null)
            {
                return UniTask.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var completion = new UniTaskCompletionSource();
            _flights.Add(new Flight
            {
                Target = target,
                From = target.position,
                To = to,
                Duration = Mathf.Max(0.01f, duration),
                Elapsed = 0f,
                Completion = completion,
                Cancellation = cancellationToken
            });

            return completion.Task;
        }

        private void Update()
        {
            var count = _flights.Count;
            if (count == 0)
            {
                return;
            }

            var dt = Time.deltaTime;

            for (var i = count - 1; i >= 0; i--)
            {
                var flight = _flights[i];

                if (flight.Cancellation.IsCancellationRequested)
                {
                    _flights.RemoveAt(i);
                    flight.Completion.TrySetCanceled(flight.Cancellation);
                    continue;
                }

                if (flight.Target == null)
                {
                    _flights.RemoveAt(i);
                    flight.Completion.TrySetResult();
                    continue;
                }

                flight.Elapsed += dt;
                var t = EasedMotion.EaseInOutCubic(flight.Elapsed / flight.Duration);
                flight.Target.position = EasedMotion.Arc(flight.From, flight.To, t, _arcHeight);

                if (flight.Elapsed >= flight.Duration)
                {
                    flight.Target.position = flight.To;
                    _flights.RemoveAt(i);
                    flight.Completion.TrySetResult();
                    continue;
                }

                _flights[i] = flight;
            }
        }

        private void OnDestroy()
        {
            for (var i = 0; i < _flights.Count; i++)
            {
                _flights[i].Completion.TrySetCanceled();
            }

            _flights.Clear();
        }
    }
}
