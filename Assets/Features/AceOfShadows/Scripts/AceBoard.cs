using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Features.AceOfShadows
{
	internal sealed class AceBoard
	{
		private readonly CardPileView[] _pileViews;
		private readonly RectTransform _flightLayer;
		private readonly CardTween _cardTween;
		private readonly CardViewPool _pool;
		private readonly Dictionary<int, CardView> _cardsById;

		public AceBoard(
			CardPileView[] pileViews,
			RectTransform flightLayer,
			CardTween cardTween,
			CardViewPool pool,
			int cardCapacity)
		{
			_pileViews = pileViews;
			_flightLayer = flightLayer;
			_cardTween = cardTween;
			_pool = pool;
			_cardsById = new Dictionary<int, CardView>(cardCapacity);
		}

		public CardPileView GetPileView(int pileId)
		{
			return _pileViews[pileId];
		}

		public bool TryGetCard(int cardId, out CardView card)
		{
			return _cardsById.TryGetValue(cardId, out card) && card != null;
		}

		public void Deal(CardPile[] piles)
		{
			for (var pileIndex = 0; pileIndex < piles.Length; pileIndex++)
			{
				var pile = piles[pileIndex];
				var view = _pileViews[pileIndex];

				for (var i = 0; i < pile.Cards.Count; i++)
				{
					var cardId = pile.Cards[i];
					var card = _pool.Get(view.StackRoot);
					card.Bind(cardId);
					view.AttachCard(card);
					_cardsById[cardId] = card;
				}
			}
		}

		public void BringFlightLayerToFront()
		{
			_flightLayer.SetAsLastSibling();
		}

		public void LiftToFlight(CardView card)
		{
			card.transform.SetParent(_flightLayer, true);
			card.transform.SetAsLastSibling();
		}

		public void RefreshCounts(CardMoveScheduler scheduler)
		{
			for (var i = 0; i < _pileViews.Length; i++)
			{
				RefreshCount(i, scheduler);
			}
		}

		public void RefreshCount(int pileId, CardMoveScheduler scheduler)
		{
			_pileViews[pileId].RefreshCount(scheduler.GetVisibleCount(pileId));
		}

		public async UniTaskVoid FlyAsync(
			CardView card,
			int sourcePileId,
			CardPileView targetView,
			Vector3 destination,
			int landingSlot,
			int cardId,
			CardMoveScheduler scheduler,
			CancellationToken cancellationToken)
		{
			var settled = false;
			try
			{
				await _cardTween.MoveAsync(
					card.transform,
					destination,
					_cardTween.DefaultDuration,
					cancellationToken);

				if (card != null)
				{
					targetView.AttachAtSlot(card, landingSlot);
				}

				scheduler.NotifyMoveCompleted(targetView.PileId, cardId, landingSlot);
				settled = true;
			}
			catch (System.OperationCanceledException)
			{
				settled = true;
				AbortFlight(card, sourcePileId, targetView.PileId, cardId, scheduler);
			}
			catch (System.Exception ex)
			{
				settled = true;
				Debug.LogException(ex);
				AbortFlight(card, sourcePileId, targetView.PileId, cardId, scheduler);
			}
			finally
			{
				if (!settled)
				{
					AbortFlight(card, sourcePileId, targetView.PileId, cardId, scheduler);
				}

				RefreshCount(sourcePileId, scheduler);
				RefreshCount(targetView.PileId, scheduler);
			}
		}

		private void AbortFlight(
			CardView card,
			int sourcePileId,
			int targetPileId,
			int cardId,
			CardMoveScheduler scheduler)
		{
			scheduler.AbortInFlightMove(sourcePileId, targetPileId, cardId);
			if (card != null)
			{
				_pileViews[sourcePileId].AttachCard(card);
			}
		}

		public void Clear()
		{
			// Do not Release into the pool here: on scene unload the host canvas is already
			// being destroyed, and SetParent to it throws. Unity destroys the card objects
			// with the hierarchy.
			_cardsById.Clear();
		}
	}
}
