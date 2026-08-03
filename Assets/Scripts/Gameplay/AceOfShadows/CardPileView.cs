using SoftGames.Core;
using TMPro;
using UnityEngine;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Presentation for one pile: overlapping layout + count label.
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

            var cardCount = CountCards();
            card.SetSortingOrder(cardCount - 1);

            if (snapLayout)
            {
                card.Rect.anchoredPosition = _overlapOffset * (cardCount - 1);
                card.Rect.localScale = Vector3.one;
                card.Rect.localRotation = Quaternion.identity;
            }
        }

        public void RefreshCount(int count)
        {
            _countLabel.color = AppColors.TextPrimary;
            _countLabel.SetText("{0}", count);
        }

        public void Relayout()
        {
            var slot = 0;
            for (var i = 0; i < _stackRoot.childCount; i++)
            {
                var child = _stackRoot.GetChild(i) as RectTransform;
                if (child == null || child.GetComponent<CardView>() == null)
                {
                    continue;
                }

                child.anchoredPosition = _overlapOffset * slot;
                child.localScale = Vector3.one;
                child.localRotation = Quaternion.identity;
                child.SetSiblingIndex(slot);
                slot++;
            }
        }

        private int CountCards()
        {
            var count = 0;
            for (var i = 0; i < _stackRoot.childCount; i++)
            {
                if (_stackRoot.GetChild(i).GetComponent<CardView>() != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
