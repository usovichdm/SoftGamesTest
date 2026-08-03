using System;
using System.Collections.Generic;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Domain stack of card ids. No Unity dependencies.
    /// </summary>
    public sealed class CardPile
    {
        private readonly List<int> _cards = new List<int>(48);

        public int Id { get; }

        public int Count
        {
            get { return _cards.Count; }
        }

        public bool IsEmpty
        {
            get { return _cards.Count == 0; }
        }

        public IReadOnlyList<int> Cards
        {
            get { return _cards; }
        }

        public CardPile(int id)
        {
            Id = id;
        }

        public void Push(int cardId)
        {
            _cards.Add(cardId);
        }

        public int Peek()
        {
            if (_cards.Count == 0)
            {
                throw new InvalidOperationException($"Pile {Id} is empty.");
            }

            return _cards[_cards.Count - 1];
        }

        public bool TryPeek(out int cardId)
        {
            if (_cards.Count == 0)
            {
                cardId = -1;
                return false;
            }

            cardId = _cards[_cards.Count - 1];
            return true;
        }

        public int Pop()
        {
            if (_cards.Count == 0)
            {
                throw new InvalidOperationException($"Pile {Id} is empty.");
            }

            var index = _cards.Count - 1;
            var cardId = _cards[index];
            _cards.RemoveAt(index);
            return cardId;
        }

        public bool TryPop(out int cardId)
        {
            if (_cards.Count == 0)
            {
                cardId = -1;
                return false;
            }

            cardId = Pop();
            return true;
        }
    }
}
