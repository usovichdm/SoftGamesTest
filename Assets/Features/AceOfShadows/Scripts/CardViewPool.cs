using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Features.AceOfShadows
{
	internal sealed class CardViewPool
	{
		private readonly ObjectPool<CardView> _pool;
		private readonly Transform _host;

		public CardViewPool(CardView prefab, Transform host, int capacity)
		{
			if (prefab == null)
			{
				throw new ArgumentNullException(nameof(prefab));
			}

			_host = host != null ? host : throw new ArgumentNullException(nameof(host));
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}

			_pool = new ObjectPool<CardView>(
				createFunc: () => UnityEngine.Object.Instantiate(prefab, _host),
				actionOnGet: card =>
				{
					card.OnRent();
					card.gameObject.SetActive(true);
				},
				actionOnRelease: card =>
				{
					card.OnReturn();
					card.gameObject.SetActive(false);
				},
				actionOnDestroy: card => UnityEngine.Object.Destroy(card.gameObject),
				collectionCheck: true,
				defaultCapacity: capacity,
				maxSize: capacity);

			Prewarm(capacity);
		}

		public CardView Get(Transform parent)
		{
			var card = _pool.Get();
			card.transform.SetParent(parent != null ? parent : _host, false);
			return card;
		}

		public void Release(CardView card)
		{
			_pool.Release(card);
		}

		private void Prewarm(int capacity)
		{
			var buffer = new CardView[capacity];
			for (var i = 0; i < capacity; i++)
			{
				buffer[i] = _pool.Get();
			}

			for (var i = 0; i < capacity; i++)
			{
				_pool.Release(buffer[i]);
			}
		}
	}
}
