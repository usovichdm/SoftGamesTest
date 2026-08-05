using System;
using System.Collections.Generic;

namespace Features.AceOfShadows
{
	internal sealed class CardMoveScheduler
	{
		public event Action BecameIdle;

		private readonly CardPile[] _piles;
		private readonly Random _random;
		private readonly int[] _pendingArrivals;
		private readonly Dictionary<int, int> _slotByCardId;
		private readonly Dictionary<int, int> _inFlightTargetByCardId;

		private int _activeAnimations;

		public bool IsIdle => _activeAnimations == 0;

		public CardMoveScheduler(CardPile[] piles, int? seed = null)
		{
			_piles = piles ?? throw new ArgumentNullException(nameof(piles));

			if (_piles.Length < 2)
			{
				throw new ArgumentException("At least two piles are required.", nameof(piles));
			}

			_pendingArrivals = new int[_piles.Length];
			_slotByCardId = new Dictionary<int, int>(144);
			_inFlightTargetByCardId = new Dictionary<int, int>(32);
			_random = seed.HasValue ? new Random(seed.Value) : new Random();

			RegisterInitialSlots();
		}

		public int GetPendingArrivals(int pileId)
		{
			if (pileId < 0 || pileId >= _pendingArrivals.Length)
			{
				return 0;
			}

			return _pendingArrivals[pileId];
		}

		public int GetVisibleCount(int pileId)
		{
			if (pileId < 0 || pileId >= _piles.Length)
			{
				return 0;
			}

			return _piles[pileId].Count;
		}

		public bool TryPlanMove(out int sourcePileId, out int targetPileId, out int cardId, out int landingSlot)
		{
			sourcePileId = -1;
			targetPileId = -1;
			cardId = -1;
			landingSlot = -1;

			if (!TryPickSource(out var source))
			{
				return false;
			}

			var target = PickTarget(source.Id);
			if (target == null)
			{
				return false;
			}

			if (!source.TryPop(out cardId))
			{
				return false;
			}

			_slotByCardId.Remove(cardId);

			landingSlot = NextLandingSlot(target.Id);
			_pendingArrivals[target.Id]++;
			_slotByCardId[cardId] = landingSlot;
			_inFlightTargetByCardId[cardId] = target.Id;

			sourcePileId = source.Id;
			targetPileId = target.Id;
			return true;
		}

		public void CancelPlannedMove(int sourcePileId, int targetPileId, int cardId)
		{
			_inFlightTargetByCardId.Remove(cardId);

			if (sourcePileId >= 0 && sourcePileId < _piles.Length)
			{
				var source = _piles[sourcePileId];
				source.Push(cardId);
				_slotByCardId[cardId] = source.Count - 1;
			}
			else
			{
				_slotByCardId.Remove(cardId);
			}

			if (targetPileId >= 0 && targetPileId < _pendingArrivals.Length && _pendingArrivals[targetPileId] > 0)
			{
				_pendingArrivals[targetPileId]--;
			}
		}

		public void NotifyMoveStarted()
		{
			_activeAnimations++;
		}

		public void NotifyMoveCompleted(int targetPileId, int cardId, int landingSlot)
		{
			_inFlightTargetByCardId.Remove(cardId);

			if (targetPileId >= 0 && targetPileId < _piles.Length)
			{
				var pile = _piles[targetPileId];
				var insertAt = CountCardsWithLowerSlot(pile, landingSlot);
				pile.Insert(insertAt, cardId);
				_slotByCardId[cardId] = landingSlot;

				if (_pendingArrivals[targetPileId] > 0)
				{
					_pendingArrivals[targetPileId]--;
				}
			}

			ReleaseAnimationSlot();
		}

		public void AbortInFlightMove(int sourcePileId, int targetPileId, int cardId)
		{
			CancelPlannedMove(sourcePileId, targetPileId, cardId);
			ReleaseAnimationSlot();
		}

		private int NextLandingSlot(int targetId)
		{
			var max = -1;

			var pile = _piles[targetId];
			for (var i = 0; i < pile.Count; i++)
			{
				if (_slotByCardId.TryGetValue(pile.Cards[i], out var slot) && slot > max)
				{
					max = slot;
				}
			}

			foreach (var pair in _inFlightTargetByCardId)
			{
				if (pair.Value != targetId)
				{
					continue;
				}

				if (_slotByCardId.TryGetValue(pair.Key, out var slot) && slot > max)
				{
					max = slot;
				}
			}

			return max + 1;
		}

		private void RegisterInitialSlots()
		{
			for (var p = 0; p < _piles.Length; p++)
			{
				var pile = _piles[p];
				for (var i = 0; i < pile.Cards.Count; i++)
				{
					_slotByCardId[pile.Cards[i]] = i;
				}
			}
		}

		private int CountCardsWithLowerSlot(CardPile pile, int landingSlot)
		{
			var insertAt = 0;
			for (var i = 0; i < pile.Count; i++)
			{
				if (_slotByCardId.TryGetValue(pile.Cards[i], out var otherSlot) && otherSlot < landingSlot)
				{
					insertAt++;
				}
			}

			return insertAt;
		}

		private void ReleaseAnimationSlot()
		{
			if (_activeAnimations <= 0)
			{
				return;
			}

			_activeAnimations--;
			if (_activeAnimations == 0)
			{
				BecameIdle?.Invoke();
			}
		}

		private bool TryPickSource(out CardPile source)
		{
			source = null;

			for (var attempt = 0; attempt < _piles.Length * 2; attempt++)
			{
				var candidate = _piles[_random.Next(_piles.Length)];
				if (CanTakeFrom(candidate))
				{
					source = candidate;
					return true;
				}
			}

			for (var i = 0; i < _piles.Length; i++)
			{
				if (CanTakeFrom(_piles[i]))
				{
					source = _piles[i];
					return true;
				}
			}

			return false;
		}

		private bool CanTakeFrom(CardPile pile)
		{
			// Skip piles that currently have a card landing — taking from under it looks wrong.
			return !pile.IsEmpty && _pendingArrivals[pile.Id] == 0;
		}

		private CardPile PickTarget(int sourceId)
		{
			for (var attempt = 0; attempt < _piles.Length * 2; attempt++)
			{
				var pile = _piles[_random.Next(_piles.Length)];
				if (pile.Id != sourceId)
				{
					return pile;
				}
			}

			for (var i = 0; i < _piles.Length; i++)
			{
				if (_piles[i].Id != sourceId)
				{
					return _piles[i];
				}
			}

			return null;
		}
	}
}
