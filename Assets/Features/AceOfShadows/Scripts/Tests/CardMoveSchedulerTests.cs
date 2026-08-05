using Features.AceOfShadows;
using FluentAssertions;
using NUnit.Framework;

namespace Features.AceOfShadows.Tests
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

			scheduler.TryPlanMove(out var source, out var target, out var cardId, out var landingSlot)
				.Should().BeTrue();
			source.Should().Be(0);
			target.Should().Be(1);
			cardId.Should().Be(7);
			landingSlot.Should().Be(0);
			a.Count.Should().Be(1);
			a.Peek().Should().Be(1);
			b.Count.Should().Be(0);
			scheduler.GetPendingArrivals(target).Should().Be(1);
			scheduler.GetVisibleCount(target).Should().Be(0);

			scheduler.NotifyMoveStarted();
			scheduler.NotifyMoveCompleted(target, cardId, landingSlot);

			b.Count.Should().Be(1);
			b.Peek().Should().Be(7);
			scheduler.GetPendingArrivals(target).Should().Be(0);
			scheduler.GetVisibleCount(target).Should().Be(1);
			scheduler.IsIdle.Should().BeTrue();
		}

		[Test]
		public void TryPlanMove_DoesNotExposeInFlightCardAsTargetTop()
		{
			var a = new CardPile(0);
			var b = new CardPile(1);
			a.Push(10);
			a.Push(20);

			var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

			scheduler.TryPlanMove(out _, out var targetA, out var firstCard, out var slotA)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			b.Count.Should().Be(0);
			firstCard.Should().Be(20);

			scheduler.TryPlanMove(out var sourceB, out var targetB, out var secondCard, out var slotB)
				.Should().BeTrue();
			sourceB.Should().Be(0);
			secondCard.Should().Be(10);
			secondCard.Should().NotBe(firstCard);

			scheduler.NotifyMoveStarted();
			scheduler.NotifyMoveCompleted(targetA, firstCard, slotA);
			scheduler.NotifyMoveCompleted(targetB, secondCard, slotB);

			a.Count.Should().Be(0);
			b.Count.Should().Be(2);
			b.Peek().Should().Be(secondCard);
		}

		[Test]
		public void NotifyMoveCompleted_OutOfOrder_PreservesReservedStackOrder()
		{
			var a = new CardPile(0);
			var b = new CardPile(1);
			a.Push(10);
			a.Push(20);

			var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

			scheduler.TryPlanMove(out _, out var targetA, out var firstCard, out var slotA)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();
			scheduler.TryPlanMove(out _, out var targetB, out var secondCard, out var slotB)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			targetB.Should().Be(targetA);
			slotA.Should().Be(0);
			slotB.Should().Be(1);

			// Later reservation lands first — domain order must still be reservation order.
			scheduler.NotifyMoveCompleted(targetB, secondCard, slotB);
			b.Count.Should().Be(1);
			b.Peek().Should().Be(secondCard);

			scheduler.NotifyMoveCompleted(targetA, firstCard, slotA);
			b.Count.Should().Be(2);
			b.Cards.Should().Equal(firstCard, secondCard);
			b.Peek().Should().Be(secondCard);
			scheduler.GetPendingArrivals(targetA).Should().Be(0);
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

			scheduler.TryPlanMove(out var source, out var target, out var cardId, out var landingSlot)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			if (scheduler.TryPlanMove(out var nextSource, out var nextTarget, out var nextCard, out _))
			{
				nextSource.Should().NotBe(target);
				scheduler.CancelPlannedMove(nextSource, nextTarget, nextCard);
			}
			else
			{
				source.Should().Be(0);
				target.Should().Be(1);
				a.Count.Should().Be(0);
				b.Count.Should().Be(2);
			}

			scheduler.NotifyMoveCompleted(target, cardId, landingSlot);
			scheduler.TryPlanMove(out _, out _, out _, out _).Should().BeTrue();
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
			scheduler.IsIdle.Should().BeFalse();

			scheduler.NotifyMoveCompleted(1, 1, landingSlot: 0);
			idleCount.Should().Be(0);
			scheduler.IsIdle.Should().BeFalse();

			scheduler.NotifyMoveCompleted(1, 2, landingSlot: 1);
			idleCount.Should().Be(1);
			scheduler.IsIdle.Should().BeTrue();
		}

		[Test]
		public void TryPlanMove_WhenAllEmpty_ReturnsFalse()
		{
			var scheduler = new CardMoveScheduler(new[] { new CardPile(0), new CardPile(1) }, seed: 3);
			scheduler.TryPlanMove(out _, out _, out _, out _).Should().BeFalse();
		}

		[Test]
		public void AbortInFlightMove_WhileOtherInFlight_AllocatesNextFreeLandingSlot()
		{
			var a = new CardPile(0);
			var b = new CardPile(1);
			a.Push(10);
			a.Push(20);
			a.Push(30);

			var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

			scheduler.TryPlanMove(out var sourceA, out var targetA, out var cardA, out var slotA)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			scheduler.TryPlanMove(out var sourceB, out var targetB, out var cardB, out var slotB)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			targetB.Should().Be(targetA);
			slotA.Should().Be(0);
			slotB.Should().Be(1);
			cardA.Should().NotBe(cardB);

			scheduler.AbortInFlightMove(sourceA, targetA, cardA);

			scheduler.TryPlanMove(out _, out var targetC, out var cardC, out var slotC)
				.Should().BeTrue();
			targetC.Should().Be(targetA);
			cardC.Should().NotBe(cardB);
			slotC.Should().Be(2);

			scheduler.NotifyMoveStarted();
			scheduler.NotifyMoveCompleted(targetB, cardB, slotB);
			scheduler.NotifyMoveCompleted(targetC, cardC, slotC);

			b.Count.Should().Be(2);
			b.Cards.Should().Equal(cardB, cardC);
		}

		[Test]
		public void CancelPlannedMove_BeforeStart_DoesNotCollideWithInFlightSlot()
		{
			var a = new CardPile(0);
			var b = new CardPile(1);
			a.Push(10);
			a.Push(20);
			a.Push(30);

			var scheduler = new CardMoveScheduler(new[] { a, b }, seed: 42);

			scheduler.TryPlanMove(out var sourceA, out var targetA, out var cardA, out var slotA)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();

			scheduler.TryPlanMove(out var sourceB, out var targetB, out var cardB, out var slotB)
				.Should().BeTrue();

			targetB.Should().Be(targetA);
			slotA.Should().Be(0);
			slotB.Should().Be(1);

			// Controller path when the card view is missing — cancel before NotifyMoveStarted.
			scheduler.CancelPlannedMove(sourceB, targetB, cardB);
			scheduler.GetPendingArrivals(targetA).Should().Be(1);

			scheduler.TryPlanMove(out _, out var targetC, out _, out var slotC)
				.Should().BeTrue();
			targetC.Should().Be(targetA);
			// Freed reservation is reusable; must not collide with still-flying A.
			slotC.Should().Be(1);
			slotC.Should().NotBe(slotA);
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

			scheduler.TryPlanMove(out var source, out var target, out var cardId, out _)
				.Should().BeTrue();
			scheduler.NotifyMoveStarted();
			a.Count.Should().Be(1);
			b.Count.Should().Be(0);
			scheduler.GetPendingArrivals(target).Should().Be(1);
			scheduler.IsIdle.Should().BeFalse();

			scheduler.AbortInFlightMove(source, target, cardId);

			a.Count.Should().Be(2);
			a.Peek().Should().Be(7);
			b.Count.Should().Be(0);
			scheduler.GetPendingArrivals(target).Should().Be(0);
			scheduler.IsIdle.Should().BeTrue();
			idleCount.Should().Be(1);

			scheduler.TryPlanMove(out _, out var targetAgain, out var cardAgain, out var slotAgain)
				.Should().BeTrue();
			cardAgain.Should().Be(7);
			scheduler.NotifyMoveStarted();
			scheduler.NotifyMoveCompleted(targetAgain, cardAgain, slotAgain);
			b.Count.Should().Be(1);
			b.Peek().Should().Be(7);
		}
	}
}
