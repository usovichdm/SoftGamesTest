using NUnit.Framework;
using SoftGames.Gameplay.AceOfShadows;

namespace SoftGames.Tests.EditMode
{
    public sealed class CardPileTests
    {
        [Test]
        public void Push_IncreasesCount_AndPeekReturnsTop()
        {
            var pile = new CardPile(0);
            pile.Push(10);
            pile.Push(20);

            Assert.AreEqual(2, pile.Count);
            Assert.AreEqual(20, pile.Peek());
        }

        [Test]
        public void Pop_RemovesTopCard()
        {
            var pile = new CardPile(1);
            pile.Push(1);
            pile.Push(2);

            Assert.AreEqual(2, pile.Pop());
            Assert.AreEqual(1, pile.Count);
            Assert.AreEqual(1, pile.Peek());
        }

        [Test]
        public void TryPop_OnEmpty_ReturnsFalse()
        {
            var pile = new CardPile(2);

            Assert.IsFalse(pile.TryPop(out var cardId));
            Assert.AreEqual(-1, cardId);
            Assert.IsTrue(pile.IsEmpty);
        }

        [Test]
        public void Insert_AtIndex_KeepsOrder()
        {
            var pile = new CardPile(0);
            pile.Push(20);
            pile.Insert(0, 10);
            pile.Insert(2, 30);

            Assert.AreEqual(3, pile.Count);
            Assert.AreEqual(10, pile.Cards[0]);
            Assert.AreEqual(20, pile.Cards[1]);
            Assert.AreEqual(30, pile.Cards[2]);
            Assert.AreEqual(30, pile.Peek());
        }

        [Test]
        public void Insert_OutOfRange_Throws()
        {
            var pile = new CardPile(0);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => pile.Insert(-1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => pile.Insert(1, 1));
        }
    }
}
