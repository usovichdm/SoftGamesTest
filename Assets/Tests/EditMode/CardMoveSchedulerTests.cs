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
            Assert.AreEqual(1, scheduler.GetPendingArrivals(target));
            Assert.AreEqual(1, scheduler.GetVisibleCount(target));

            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveCompleted(target, cardId, landingSlot);

            Assert.AreEqual(1, b.Count);
            Assert.AreEqual(7, b.Peek());
            Assert.AreEqual(0, scheduler.GetPendingArrivals(target));
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

            Assert.IsTrue(scheduler.TryPlanMove(out _, out var targetA, out var firstCard, out var slotA));
            scheduler.NotifyMoveStarted();

            Assert.AreEqual(0, b.Count);
            Assert.AreEqual(20, firstCard);

            Assert.IsTrue(scheduler.TryPlanMove(out var sourceB, out var targetB, out var secondCard, out var slotB));
            Assert.AreEqual(0, sourceB);
            Assert.AreEqual(10, secondCard);
            Assert.AreNotEqual(firstCard, secondCard);

            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveCompleted(targetA, firstCard, slotA);
            scheduler.NotifyMoveCompleted(targetB, secondCard, slotB);

            Assert.AreEqual(0, a.Count);
            Assert.AreEqual(2, b.Count);
            Assert.AreEqual(secondCard, b.Peek());
        }

        [Test]
        public void NotifyMoveCompleted_OutOfOrder_PreservesReservedStackOrder()
        {
            var a = new CardPile(0);
            var b = new CardPile(1);
            a.Push(10);
            a.Push(20);

            var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

            Assert.IsTrue(scheduler.TryPlanMove(out _, out var targetA, out var firstCard, out var slotA));
            scheduler.NotifyMoveStarted();
            Assert.IsTrue(scheduler.TryPlanMove(out _, out var targetB, out var secondCard, out var slotB));
            scheduler.NotifyMoveStarted();

            Assert.AreEqual(targetA, targetB);
            Assert.AreEqual(0, slotA);
            Assert.AreEqual(1, slotB);

            // Later reservation lands first — domain order must still be reservation order.
            scheduler.NotifyMoveCompleted(targetB, secondCard, slotB);
            Assert.AreEqual(1, b.Count);
            Assert.AreEqual(secondCard, b.Peek());

            scheduler.NotifyMoveCompleted(targetA, firstCard, slotA);
            Assert.AreEqual(2, b.Count);
            Assert.AreEqual(firstCard, b.Cards[0]);
            Assert.AreEqual(secondCard, b.Cards[1]);
            Assert.AreEqual(secondCard, b.Peek());
            Assert.AreEqual(0, scheduler.GetPendingArrivals(targetA));
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

            Assert.IsTrue(scheduler.TryPlanMove(out var source, out var target, out var cardId, out var landingSlot));
            scheduler.NotifyMoveStarted();

            if (scheduler.TryPlanMove(out var nextSource, out var nextTarget, out var nextCard, out _))
            {
                Assert.AreNotEqual(target, nextSource);
                scheduler.CancelPlannedMove(nextSource, nextTarget, nextCard);
            }
            else
            {
                Assert.AreEqual(0, source);
                Assert.AreEqual(1, target);
                Assert.AreEqual(0, a.Count);
                Assert.AreEqual(2, b.Count);
            }

            scheduler.NotifyMoveCompleted(target, cardId, landingSlot);
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

            scheduler.NotifyMoveCompleted(1, 1, landingSlot: 0);
            Assert.AreEqual(0, idleCount);
            Assert.IsFalse(scheduler.IsIdle);

            scheduler.NotifyMoveCompleted(1, 2, landingSlot: 1);
            Assert.AreEqual(1, idleCount);
            Assert.IsTrue(scheduler.IsIdle);
        }

        [Test]
        public void TryPlanMove_WhenAllEmpty_ReturnsFalse()
        {
            var scheduler = new CardMoveScheduler(new[] { new CardPile(0), new CardPile(1) }, seed: 3);
            Assert.IsFalse(scheduler.TryPlanMove(out _, out _, out _, out _));
        }

        [Test]
        public void AbortInFlightMove_RestoresSource_AndDoesNotPushTarget()
        {
            var a = new CardPile(0);
            var b = new CardPile(1);
            a.Push(1);
            a.Push(7);

            var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);
            var idleCount = 0;
            scheduler.BecameIdle += () => idleCount++;

            Assert.IsTrue(scheduler.TryPlanMove(out var source, out var target, out var cardId, out _));
            scheduler.NotifyMoveStarted();
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual(0, b.Count);
            Assert.AreEqual(1, scheduler.GetPendingArrivals(target));
            Assert.IsFalse(scheduler.IsIdle);

            scheduler.AbortInFlightMove(source, target, cardId);

            Assert.AreEqual(2, a.Count);
            Assert.AreEqual(7, a.Peek());
            Assert.AreEqual(0, b.Count);
            Assert.AreEqual(0, scheduler.GetPendingArrivals(target));
            Assert.IsTrue(scheduler.IsIdle);
            Assert.AreEqual(1, idleCount);

            Assert.IsTrue(scheduler.TryPlanMove(out _, out var targetAgain, out var cardAgain, out var slotAgain));
            Assert.AreEqual(7, cardAgain);
            scheduler.NotifyMoveStarted();
            scheduler.NotifyMoveCompleted(targetAgain, cardAgain, slotAgain);
            Assert.AreEqual(1, b.Count);
            Assert.AreEqual(7, b.Peek());
        }
    }
}
