using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Visual representation of a single playing card.
    /// Suit glyphs are sprites — LiberationSans has no ♠♥♦♣ codepoints.
    /// </summary>
    public sealed class CardView : MonoBehaviour
    {
        private static readonly string[] Ranks =
        {
            "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
        };

        private static readonly string[] SuitNames = { "S", "H", "D", "C" };

        [SerializeField]
        private RectTransform _rect;

        [SerializeField]
        private Image _face;

        [SerializeField]
        private Image _frameOuter;

        [SerializeField]
        private Image _frameAccent;

        [SerializeField]
        private Image _shadow;

        [SerializeField]
        private TMP_Text _rankLabel;

        [SerializeField]
        private Image _suitImage;

        [SerializeField]
        private Sprite[] _suitSprites;

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
            var isRed = suitIndex == 1 || suitIndex == 2;
            var ink = isRed ? AppColors.SuitRed : AppColors.SuitBlack;

            _face.color = AppColors.CardFace;
            _shadow.color = AppColors.CardShadow;
            _frameOuter.color = AppColors.CardBorder;
            _frameAccent.color = AppColors.CardBorderAccent;

            _rankLabel.text = rank;
            _rankLabel.color = ink;

            _suitImage.sprite = _suitSprites[suitIndex];
            _suitImage.color = ink;
            _suitImage.preserveAspect = true;

            _cornerLabel.text = rank;
            _cornerLabel.color = ink;

            gameObject.name = $"Card_{rank}{SuitNames[suitIndex]}_{cardId}";
        }

        public void PrepareForPool()
        {
            CardId = -1;
            gameObject.name = "Card_Pooled";

            _rect.anchoredPosition = Vector2.zero;
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
        }

        public void PrepareForRent()
        {
            _rect.anchoredPosition = Vector2.zero;
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
        }

        public void SetSortingOrder(int order)
        {
            transform.SetSiblingIndex(order);
        }
    }
}
