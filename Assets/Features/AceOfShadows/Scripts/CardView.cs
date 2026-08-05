using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.AceOfShadows
{
	internal sealed class CardView : MonoBehaviour
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

		public RectTransform Rect => _rect;

		public void Bind(int cardId)
		{
			var rankIndex = cardId % 13;
			var suitIndex = (cardId / 13) % 4;
			var rank = Ranks[rankIndex];
			var isRed = suitIndex == 1 || suitIndex == 2;
			var ink = isRed ? AceColors.SuitRed : AceColors.SuitBlack;

			_face.color = AceColors.CardFace;
			_shadow.color = AceColors.CardShadow;
			_frameOuter.color = AceColors.CardBorder;
			_frameAccent.color = AceColors.CardBorderAccent;

			_rankLabel.text = rank;
			_rankLabel.color = ink;

			_suitImage.sprite = _suitSprites[suitIndex];
			_suitImage.color = ink;
			_suitImage.preserveAspect = true;

			_cornerLabel.text = rank;
			_cornerLabel.color = ink;

			gameObject.name = $"Card_{rank}{SuitNames[suitIndex]}_{cardId}";
		}

		public void OnReturn()
		{
			gameObject.name = "Card_Pooled";

			_rect.anchoredPosition = Vector2.zero;
			_rect.localScale = Vector3.one;
			_rect.localRotation = Quaternion.identity;
		}

		public void OnRent()
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
