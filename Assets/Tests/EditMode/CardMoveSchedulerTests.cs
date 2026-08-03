using NUnit.Framework;
using SoftGames.Gameplay.AceOfShadows;

namespace SoftGames.Tests.EditMode
{
    public sealed class CardMoveSchedulerTests
    {
        [Test]
        public void TryPlanMove_TakesOnlySourceTop_AndDefersTargetPush()
        {
            var a = new CardPile(0);
            var b = new CardPile(1);
            a.Push(1);
            a.Push(7);

            var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

            Assert.IsTrue(scheduler.TryPlanMove(out var source, out var target, out var cardId, out var landingSlot));
            Assert.AreEqual(0, source);
            Assert.AreEqual(1, target);
            Assert.AreEqual(7, cardId);
            Assert.AreEqual(0, landingSlot);
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual(1, a.Peek());
            Assert.AreEqual(0, b.Count);

            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveCompleted(target, cardId);

            Assert.AreEqual(1, b.Count);
            Assert.AreEqual(7, b.Peek());
            Assert.IsTrue(scheduler.IsIdle);
        }

        [Test]
        public void TryPlanMove_DoesNotExposeInFlightCardAsTargetTop()
        {
            var a = new CardPile(0);
            var b = new CardPile(1);
            a.Push(10);
            a.Push(20);

            var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

            Assert.IsTrue(scheduler.TryPlanMove(out _, out var targetA, out var firstCard, out _));
            scheduler.NotifyMoveStarted();

            // First card is in flight toward B and must not be poppable from B yet.
            Assert.AreEqual(0, b.Count);
            Assert.AreEqual(20, firstCard);

            Assert.IsTrue(scheduler.TryPlanMove(out var sourceB, out var targetB, out var secondCard, out _));
            Assert.AreEqual(0, sourceB);
            Assert.AreEqual(10, secondCard);
            Assert.AreNotEqual(firstCard, secondCard);

            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveCompleted(targetA, firstCard);
            scheduler.NotifyMoveCompleted(targetB, secondCard);

            Assert.AreEqual(0, a.Count);
            Assert.AreEqual(2, b.Count);
            Assert.AreEqual(secondCard, b.Peek());
        }

        [Test]
        public void TryPlanMove_DoesNotTakeFromPileWithPendingArrival()
        {
            var a = new CardPile(0);
            var b = new CardPile(1);
            a.Push(10);
            b.Push(20);
            b.Push(30);

            var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

            Assert.IsTrue(scheduler.TryPlanMove(out var source, out var target, out var cardId, out _));
            scheduler.NotifyMoveStarted();

            // While a card is landing on `target`, never deal from that pile.
            if (scheduler.TryPlanMove(out var nextSource, out var nextTarget, out var nextCard, out _))
            {
                Assert.AreNotEqual(target, nextSource);
                scheduler.CancelPlannedMove(nextSource, nextTarget, nextCard);
            }
            else
            {
                // Only possible when the sole remaining cards sit under a pending arrival.
                Assert.AreEqual(0, source);
                Assert.AreEqual(1, target);
                Assert.AreEqual(0, a.Count);
                Assert.AreEqual(2, b.Count);
            }

            scheduler.NotifyMoveCompleted(target, cardId);
            Assert.IsTrue(scheduler.TryPlanMove(out _, out _, out _, out _));
        }

        [Test]
        public void BecameIdle_Fires_WhenLastAnimationCompletes()
        {
            var piles = new[] { new CardPile(0), new CardPile(1) };
            piles[0].Push(1);
            var scheduler = new CardMoveScheduler(piles, seed: 1);

            var idleCount = 0;
            scheduler.BecameIdle += () => idleCount++;

            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveStarted();
            Assert.IsFalse(scheduler.IsIdle);

            scheduler.NotifyMoveCompleted(1, 1);
            Assert.AreEqual(0, idleCount);
            Assert.IsFalse(scheduler.IsIdle);

            scheduler.NotifyMoveCompleted(1, 2);
            Assert.AreEqual(1, idleCount);
            Assert.IsTrue(scheduler.IsIdle);
        }

        [Test]
        public void TryPlanMove_WhenAllEmpty_ReturnsFalse()
        {
            var scheduler = new CardMoveScheduler(new[] { new CardPile(0), new CardPile(1) }, seed: 3);
            Assert.IsFalse(scheduler.TryPlanMove(out _, out _, out _, out _));
        }
    }
}
