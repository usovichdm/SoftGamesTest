using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SoftGames.Animation;
using SoftGames.Core;
using SoftGames.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Spawns 144 cards into piles and drives the 1s move / 2s tween loop.
    /// </summary>
    public sealed class AceOfShadowsController : MonoBehaviour
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

        private readonly Dictionary<int, CardView> _cardsById = new Dictionary<int, CardView>(TotalCards);

        private CardPile[] _piles;
        private CardMoveScheduler _scheduler;
        private CancellationTokenSource _loopCts;

        private void Awake()
        {
            _background.color = AppColors.BackgroundDeep;

            _title.text = "Ace of Shadows";
            _title.color = AppColors.TextPrimary;
        }

        private void Start()
        {
            BuildDomain();
            EnsureFlightLayer();
            SpawnCards();
            RefreshAllCounts();

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

        private void SpawnCards()
        {
            for (var pileIndex = 0; pileIndex < _piles.Length; pileIndex++)
            {
                var pile = _piles[pileIndex];
                var view = _pileViews[pileIndex];

                for (var i = 0; i < pile.Cards.Count; i++)
                {
                    var cardId = pile.Cards[i];
                    var card = Instantiate(_cardPrefab, view.StackRoot);
                    card.Bind(cardId);
                    view.AttachCard(card, snapLayout: true);
                    _cardsById[cardId] = card;
                }
            }
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
                        await UniTask.Delay(
                            System.TimeSpan.FromSeconds(MoveIntervalSeconds),
                            cancellationToken: cancellationToken);
                    }

                    await UniTask.Delay(
                        System.TimeSpan.FromSeconds(MoveIntervalSeconds),
                        cancellationToken: cancellationToken);

                    if (!_scheduler.TryPlanMove(out var sourceId, out var targetId, out var cardId, out var landingSlot))
                    {
                        continue;
                    }

                    if (!_cardsById.TryGetValue(cardId, out var card) || card == null)
                    {
                        _scheduler.CancelPlannedMove(sourceId, targetId, cardId);
                        continue;
                    }

                    var sourceView = _pileViews[sourceId];
                    var targetView = _pileViews[targetId];
                    var destination = targetView.WorldPositionForSlot(landingSlot);

                    // Lift onto the shared flight layer so the card stays above every pile.
                    card.transform.SetParent(_flightLayer, true);
                    card.transform.SetAsLastSibling();
                    sourceView.Relayout();

                    sourceView.RefreshCount(_piles[sourceId].Count);
                    targetView.RefreshCount(_piles[targetId].Count);

                    _scheduler.NotifyMoveStarted();
                    movesInWave++;

                    FlyCardAsync(
                            card,
                            sourceId,
                            targetView,
                            destination,
                            landingSlot,
                            cardId,
                            cancellationToken)
                        .Forget();
                }
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        private async UniTaskVoid FlyCardAsync(
            CardView card,
            int sourcePileId,
            CardPileView targetView,
            Vector3 destination,
            int landingSlot,
            int cardId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _cardTween.MoveAsync(
                    card.transform,
                    destination,
                    _cardTween.DefaultDuration,
                    cancellationToken);

                if (card != null)
                {
                    targetView.AttachCard(card, snapLayout: false);
                    card.Rect.anchoredPosition = targetView.OverlapOffset * landingSlot;
                    card.Rect.localScale = Vector3.one;
                    card.Rect.localRotation = Quaternion.identity;
                    card.SetSortingOrder(landingSlot);
                }

                _scheduler.NotifyMoveCompleted(targetView.PileId, cardId);
                targetView.RefreshCount(_piles[targetView.PileId].Count);
            }
            catch (System.OperationCanceledException)
            {
                _scheduler.AbortInFlightMove(sourcePileId, targetView.PileId, cardId);
            }
        }

        private void EnsureFlightLayer()
        {
            if (_flightLayer != null)
            {
                _flightLayer.SetAsLastSibling();
                return;
            }

            // Fallback when the scene has not wired FlightLayer: create under Piles root
            // (Stack -> Pile -> Piles) so flying cards stay above every pile.
            var pilesRoot = (RectTransform)_pileViews[0].StackRoot.parent.parent;
            var existing = pilesRoot.Find("FlightLayer") as RectTransform;
            if (existing != null)
            {
                _flightLayer = existing;
            }
            else
            {
                var go = new GameObject("FlightLayer", typeof(RectTransform));
                _flightLayer = (RectTransform)go.transform;
                _flightLayer.SetParent(pilesRoot, false);
                _flightLayer.anchorMin = Vector2.zero;
                _flightLayer.anchorMax = Vector2.one;
                _flightLayer.offsetMin = Vector2.zero;
                _flightLayer.offsetMax = Vector2.zero;
                _flightLayer.pivot = new Vector2(0.5f, 0.5f);
            }

            _flightLayer.SetAsLastSibling();
        }

        private void RefreshAllCounts()
        {
            for (var i = 0; i < _piles.Length; i++)
            {
                _pileViews[i].RefreshCount(_piles[i].Count);
            }
        }

        private void OnBecameIdle()
        {
            _screenMessage.Show("All animations finished");
        }
    }
}
