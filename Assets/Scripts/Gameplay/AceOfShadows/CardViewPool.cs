using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Reuses <see cref="CardView"/> instances to avoid Instantiate/Destroy churn on
    /// deal, restart, or scene reload.
    /// </summary>
    public sealed class CardViewPool : IDisposable
    {
        private readonly CardView _prefab;
        private readonly Transform _inactiveRoot;
        private readonly Stack<CardView> _available;
        private readonly List<CardView> _all;
        private readonly int _maxSize;

        private bool _disposed;

        public int AvailableCount
        {
            get { return _available.Count; }
        }

        public int CreatedCount
        {
            get { return _all.Count; }
        }

        public CardViewPool(CardView prefab, Transform inactiveRoot, int prewarmCount, int maxSize = 0)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _inactiveRoot = inactiveRoot != null
                ? inactiveRoot
                : throw new ArgumentNullException(nameof(inactiveRoot));

            if (prewarmCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(prewarmCount));
            }

            _maxSize = maxSize > 0 ? maxSize : Math.Max(prewarmCount, 1);
            _available = new Stack<CardView>(_maxSize);
            _all = new List<CardView>(_maxSize);

            Prewarm(prewarmCount);
        }

        public CardView Rent(Transform parent)
        {
            ThrowIfDisposed();

            var card = _available.Count > 0 ? _available.Pop() : CreateInstance();
            var parentTransform = parent != null ? parent : _inactiveRoot;
            card.transform.SetParent(parentTransform, false);
            card.PrepareForRent();
            card.gameObject.SetActive(true);
            return card;
        }

        public void Return(CardView card)
        {
            if (_disposed || card == null)
            {
                return;
            }

            card.PrepareForPool();
            card.gameObject.SetActive(false);
            card.transform.SetParent(_inactiveRoot, false);

            if (_available.Count < _maxSize)
            {
                _available.Push(card);
            }
            else
            {
                _all.Remove(card);
                UnityEngine.Object.Destroy(card.gameObject);
            }
        }

        public void ReturnAll(IEnumerable<CardView> cards)
        {
            if (cards == null)
            {
                return;
            }

            foreach (var card in cards)
            {
                Return(card);
            }
        }

        public void Dispose()
        {
            Dispose(destroyInstances: true);
        }

        /// <param name="destroyInstances">
        /// False when the host MonoBehaviour is already tearing down — Unity will destroy the hierarchy.
        /// </param>
        public void Dispose(bool destroyInstances)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _available.Clear();

            if (destroyInstances)
            {
                for (var i = 0; i < _all.Count; i++)
                {
                    var card = _all[i];
                    if (card != null)
                    {
                        UnityEngine.Object.Destroy(card.gameObject);
                    }
                }
            }

            _all.Clear();
        }

        private void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var card = CreateInstance();
                card.PrepareForPool();
                card.gameObject.SetActive(false);
                card.transform.SetParent(_inactiveRoot, false);
                _available.Push(card);
            }
        }

        private CardView CreateInstance()
        {
            var card = UnityEngine.Object.Instantiate(_prefab, _inactiveRoot);
            _all.Add(card);
            return card;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CardViewPool));
            }
        }
    }
}
