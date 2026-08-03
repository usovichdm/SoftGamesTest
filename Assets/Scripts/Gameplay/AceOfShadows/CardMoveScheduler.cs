using System;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Chooses source/target piles and tracks in-flight animations.
    /// Only the current top card of a pile can leave; arrival is committed when the tween ends
    /// so in-flight cards are never treated as another pile's top. Piles with a pending landing
    /// are not used as sources (avoids dealing a card out from under an arriving one).
    /// </summary>
    public sealed class CardMoveScheduler
    {
        public event Action BecameIdle;

        private readonly CardPile[] _piles;
        private readonly Random _random;
        private readonly int[] _pendingArrivals;

        private int _activeAnimations;

        public int ActiveAnimations
        {
            get { return _activeAnimations; }
        }

        public bool IsIdle
        {
            get { return _activeAnimations == 0; }
        }

        public CardMoveScheduler(CardPile[] piles, int? seed = null)
        {
            _piles = piles ?? throw new ArgumentNullException(nameof(piles));
            if (_piles.Length < 2)
            {
                throw new ArgumentException("At least two piles are required.", nameof(piles));
            }

            _pendingArrivals = new int[_piles.Length];
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
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

            // Reserve the visual/domain slot now; Push happens on animation complete.
            landingSlot = target.Count + _pendingArrivals[target.Id];
            _pendingArrivals[target.Id]++;

            sourcePileId = source.Id;
            targetPileId = target.Id;
            return true;
        }

        public void CancelPlannedMove(int sourcePileId, int targetPileId, int cardId)
        {
            if (sourcePileId >= 0 && sourcePileId < _piles.Length)
            {
                _piles[sourcePileId].Push(cardId);
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

        public void NotifyMoveCompleted(int targetPileId, int cardId)
        {
            if (targetPileId >= 0 && targetPileId < _piles.Length)
            {
                _piles[targetPileId].Push(cardId);
                if (_pendingArrivals[targetPileId] > 0)
                {
                    _pendingArrivals[targetPileId]--;
                }
            }

            ReleaseAnimationSlot();
        }

        /// <summary>
        /// Aborts an in-flight move after <see cref="NotifyMoveStarted"/>: restores the card
        /// to the source pile and frees the animation slot without pushing to the target.
        /// </summary>
        public void AbortInFlightMove(int sourcePileId, int targetPileId, int cardId)
        {
            CancelPlannedMove(sourcePileId, targetPileId, cardId);
            ReleaseAnimationSlot();
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
