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
    }
}
