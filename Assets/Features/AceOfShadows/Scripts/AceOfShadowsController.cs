using Common.Core;
using Cysharp.Threading.Tasks;
using Common.UI;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.AceOfShadows
{
	internal sealed class AceOfShadowsController : MonoBehaviour
	{
		private const int TotalCards = 144;
		private const int PileCount = 4;
		private const float MoveIntervalSeconds = 1f;
		private const int MovesPerWave = 8;

		[SerializeField]
		private CardView _cardPrefab;

		[SerializeField]
		private CardPileView[] _pileViews;

		[SerializeField]
		private CardTween _cardTween;

		[SerializeField]
		private ScreenMessage _screenMessage;

		[SerializeField]
		private TMP_Text _title;

		[SerializeField]
		private Image _background;

		[SerializeField]
		private RectTransform _flightLayer;

		private CardPile[] _piles;
		private CardMoveScheduler _scheduler;
		private AceBoard _board;
		private CancellationTokenSource _loopCts;

		private void Awake()
		{
			_background.color = AppColors.BackgroundDeep;

			_title.text = "Ace of Shadows";
			_title.color = AppColors.TextPrimary;

			var pool = new CardViewPool(_cardPrefab, transform, TotalCards);
			_board = new AceBoard(_pileViews, _flightLayer, _cardTween, pool, TotalCards);
		}

		private void Start()
		{
			BuildDomain();
			_board.BringFlightLayerToFront();
			_board.Deal(_piles);
			_board.RefreshCounts(_scheduler);

			_scheduler.BecameIdle += OnBecameIdle;
			_loopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			MoveLoopAsync(_loopCts.Token).Forget();
		}

		private void OnDestroy()
		{
			if (_scheduler != null)
			{
				_scheduler.BecameIdle -= OnBecameIdle;
			}

			if (_loopCts != null)
			{
				_loopCts.Cancel();
				_loopCts.Dispose();
				_loopCts = null;
			}

			_board.Clear();
			_board = null;
		}

		private void BuildDomain()
		{
			_piles = new CardPile[PileCount];
			for (var i = 0; i < PileCount; i++)
			{
				_piles[i] = new CardPile(i);
				_pileViews[i].Configure(i);
			}

			for (var cardId = 0; cardId < TotalCards; cardId++)
			{
				_piles[cardId % PileCount].Push(cardId);
			}

			_scheduler = new CardMoveScheduler(_piles);
		}

		private async UniTaskVoid MoveLoopAsync(CancellationToken cancellationToken)
		{
			var movesInWave = 0;

			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					if (movesInWave >= MovesPerWave)
					{
						await UniTask.WaitUntil(() => _scheduler.IsIdle, cancellationToken: cancellationToken);
						movesInWave = 0;
					}

					await UniTask.Delay(
						System.TimeSpan.FromSeconds(MoveIntervalSeconds),
						cancellationToken: cancellationToken);

					if (!_scheduler.TryPlanMove(out var sourceId, out var targetId, out var cardId, out var landingSlot))
					{
						continue;
					}

					if (!_board.TryGetCard(cardId, out var card))
					{
						_scheduler.CancelPlannedMove(sourceId, targetId, cardId);
						continue;
					}

					var targetView = _board.GetPileView(targetId);
					var destination = targetView.WorldPositionForSlot(landingSlot);

					_board.LiftToFlight(card);
					_board.RefreshCount(sourceId, _scheduler);
					_board.RefreshCount(targetId, _scheduler);

					_scheduler.NotifyMoveStarted();
					movesInWave++;

					_board.FlyAsync(
							card,
							sourceId,
							targetView,
							destination,
							landingSlot,
							cardId,
							_scheduler,
							cancellationToken)
						.Forget();
				}
			}
			catch (System.OperationCanceledException) { }
		}

		private void OnBecameIdle()
		{
			_screenMessage.Show("All animations finished");
		}
	}
}
