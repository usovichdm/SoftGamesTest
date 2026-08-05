using Features.AceOfShadows;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Features.AceOfShadows.Tests
{
	public sealed class CardPileTests
	{
		[Test]
		public void Push_IncreasesCount_AndPeekReturnsTop()
		{
			var pile = new CardPile(0);
			pile.Push(10);
			pile.Push(20);

			pile.Count.Should().Be(2);
			pile.Peek().Should().Be(20);
		}

		[Test]
		public void Pop_RemovesTopCard()
		{
			var pile = new CardPile(1);
			pile.Push(1);
			pile.Push(2);

			pile.Pop().Should().Be(2);
			pile.Count.Should().Be(1);
			pile.Peek().Should().Be(1);
		}

		[Test]
		public void TryPop_OnEmpty_ReturnsFalse()
		{
			var pile = new CardPile(2);

			pile.TryPop(out var cardId).Should().BeFalse();
			cardId.Should().Be(-1);
			pile.IsEmpty.Should().BeTrue();
		}

		[Test]
		public void Insert_AtIndex_KeepsOrder()
		{
			var pile = new CardPile(0);
			pile.Push(20);
			pile.Insert(0, 10);
			pile.Insert(2, 30);

			pile.Count.Should().Be(3);
			pile.Cards.Should().Equal(10, 20, 30);
			pile.Peek().Should().Be(30);
		}

		[Test]
		public void Insert_OutOfRange_Throws()
		{
			var pile = new CardPile(0);

			Action insertBefore = () => pile.Insert(-1, 1);
			Action insertPastEnd = () => pile.Insert(1, 1);

			insertBefore.Should().Throw<ArgumentOutOfRangeException>();
			insertPastEnd.Should().Throw<ArgumentOutOfRangeException>();
		}
	}
}
