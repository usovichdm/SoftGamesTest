using Common.Core;
using TMPro;
using UnityEngine;

namespace Features.AceOfShadows
{
	internal sealed class CardPileView : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _stackRoot;

		[SerializeField]
		private TMP_Text _countLabel;

		[SerializeField]
		private Vector2 _overlapOffset = new Vector2(0f, -14f);

		[SerializeField]
		private int _pileId;

		public int PileId => _pileId;

		public RectTransform StackRoot => _stackRoot;

		public Vector3 WorldPositionForSlot(int slotIndex)
		{
			var local = _overlapOffset * Mathf.Max(0, slotIndex);
			return _stackRoot.TransformPoint(local);
		}

		public void Configure(int pileId)
		{
			_pileId = pileId;
			gameObject.name = $"Pile_{pileId}";
		}

		public void AttachCard(CardView card)
		{
			card.transform.SetParent(_stackRoot, true);
			var slot = Mathf.Max(0, _stackRoot.childCount - 1);
			ApplySlot(card, slot);
		}

		public void AttachAtSlot(CardView card, int landingSlot)
		{
			card.transform.SetParent(_stackRoot, true);
			ApplySlot(card, landingSlot);
		}

		public void RefreshCount(int count)
		{
			_countLabel.color = AppColors.TextPrimary;
			_countLabel.SetText("{0}", count);
		}

		private void ApplySlot(CardView card, int slot)
		{
			card.SetSortingOrder(slot);
			var sibling = Mathf.Clamp(slot, 0, Mathf.Max(0, _stackRoot.childCount - 1));
			card.transform.SetSiblingIndex(sibling);
			card.Rect.anchoredPosition = _overlapOffset * slot;
			card.Rect.localScale = Vector3.one;
			card.Rect.localRotation = Quaternion.identity;
		}
	}
}
