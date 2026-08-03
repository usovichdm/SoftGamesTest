using SoftGames.Core;
using TMPro;
using UnityEngine;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Presentation for one pile: overlapping layout + count label.
    /// StackRoot children are assumed to be cards only.
    /// </summary>
    public sealed class CardPileView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _stackRoot;

        [SerializeField]
        private TMP_Text _countLabel;

        [SerializeField]
        private Vector2 _overlapOffset = new Vector2(0f, -14f);

        [SerializeField]
        private int _pileId;

        public int PileId
        {
            get { return _pileId; }
        }

        public RectTransform StackRoot
        {
            get { return _stackRoot; }
        }

        public Vector2 OverlapOffset
        {
            get { return _overlapOffset; }
        }

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

        public void AttachCard(CardView card, bool snapLayout)
        {
            card.transform.SetParent(_stackRoot, true);
            var slot = Mathf.Max(0, _stackRoot.childCount - 1);
            ApplySlot(card, slot, snapLayout);
        }

        public void AttachAtSlot(CardView card, int landingSlot, bool snapLayout)
        {
            card.transform.SetParent(_stackRoot, true);
            var sibling = Mathf.Clamp(landingSlot, 0, _stackRoot.childCount - 1);
            card.transform.SetSiblingIndex(sibling);
            ApplySlot(card, landingSlot, snapLayout);
        }

        public void RefreshCount(int count)
        {
            _countLabel.color = AppColors.TextPrimary;
            _countLabel.SetText("{0}", count);
        }

        public void Relayout()
        {
            for (var i = 0; i < _stackRoot.childCount; i++)
            {
                var child = _stackRoot.GetChild(i) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                child.anchoredPosition = _overlapOffset * i;
                child.localScale = Vector3.one;
                child.localRotation = Quaternion.identity;

                if (child.TryGetComponent(out CardView card))
                {
                    card.SetSortingOrder(i);
                }
            }
        }

        private void ApplySlot(CardView card, int slot, bool snapLayout)
        {
            card.SetSortingOrder(slot);

            if (!snapLayout)
            {
                return;
            }

            card.Rect.anchoredPosition = _overlapOffset * slot;
            card.Rect.localScale = Vector3.one;
            card.Rect.localRotation = Quaternion.identity;
        }
    }
}
