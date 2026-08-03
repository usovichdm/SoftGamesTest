using System;
using System.Collections.Generic;

namespace SoftGames.Utilities
{
    /// <summary>
    /// Minimal generic pool. Avoids per-frame allocations when views are recycled.
    /// </summary>
    public sealed class ObjectPool<T>
        where T : class
    {
        private readonly Stack<T> _available = new Stack<T>(32);
        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;

        public ObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onRelease = null, int prewarm = 0)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _onGet = onGet;
            _onRelease = onRelease;

            for (var i = 0; i < prewarm; i++)
            {
                _available.Push(_create());
            }
        }

        public T Get()
        {
            var item = _available.Count > 0 ? _available.Pop() : _create();
            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            _onRelease?.Invoke(item);
            _available.Push(item);
        }
    }
}
