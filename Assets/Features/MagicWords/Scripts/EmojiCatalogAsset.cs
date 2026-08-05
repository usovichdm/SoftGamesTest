using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Features.MagicWords
{
	[CreateAssetMenu(fileName = "EmojiCatalog", menuName = "Magic Words/Emoji Catalog")]
	internal sealed class EmojiCatalogAsset : ScriptableObject
	{
		[Serializable]
		internal struct Entry
		{
			public string Key;
			public string Unicode;
			public string AtlasId;
		}

		[SerializeField]
		private Entry[] _entries = Array.Empty<Entry>();

		[SerializeField]
		private TMP_SpriteAsset _spriteAsset;

		private Dictionary<string, Entry> _map;

		public TMP_SpriteAsset SpriteAsset => _spriteAsset;

		private void OnEnable()
		{
			RebuildMap();
		}

		public bool TryGet(string key, out Entry entry)
		{
			EnsureMap();
			if (string.IsNullOrEmpty(key))
			{
				entry = default;
				return false;
			}

			return _map.TryGetValue(key, out entry);
		}

		internal void SetEntries(Entry[] entries)
		{
			_entries = entries ?? Array.Empty<Entry>();
			RebuildMap();
		}

		private void EnsureMap()
		{
			if (_map == null)
			{
				RebuildMap();
			}
		}

		private void RebuildMap()
		{
			_map = new Dictionary<string, Entry>(_entries.Length, StringComparer.Ordinal);
			for (var i = 0; i < _entries.Length; i++)
			{
				var entry = _entries[i];
				if (string.IsNullOrWhiteSpace(entry.Key))
				{
					continue;
				}

				_map[entry.Key.Trim()] = entry;
			}
		}
	}
}
