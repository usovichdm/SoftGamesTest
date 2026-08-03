using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Visual representation of a single playing card.
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly string[] Ranks =
        {
            "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
        };

        private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };

        [SerializeField]
        private RectTransform _rect;

        [SerializeField]
        private Image _face;

        [SerializeField]
        private Image _shadow;

        [SerializeField]
        private TMP_Text _rankLabel;

        [SerializeField]
        private TMP_Text _suitLabel;

        [SerializeField]
        private TMP_Text _cornerLabel;

        public int CardId { get; private set; } = -1;

        public RectTransform Rect
        {
            get { return _rect; }
        }

        public void Bind(int cardId)
        {
            CardId = cardId;

            var rankIndex = cardId % 13;
            var suitIndex = (cardId / 13) % 4;
            var rank = Ranks[rankIndex];
            var suit = Suits[suitIndex];
            var isRed = suitIndex == 1 || suitIndex == 2;
            var ink = isRed ? AppColors.SuitRed : AppColors.SuitBlack;

            _face.color = AppColors.CardFace;
            _shadow.color = AppColors.CardShadow;

            _rankLabel.text = rank;
            _rankLabel.color = ink;

            _suitLabel.text = suit;
            _suitLabel.color = ink;

            _cornerLabel.text = rank + suit;
            _cornerLabel.color = ink;

            gameObject.name = $"Card_{rank}{suit}_{cardId}";
        }

        public void SetSortingOrder(int order)
        {
            transform.SetSiblingIndex(order);
        }
    }
}
