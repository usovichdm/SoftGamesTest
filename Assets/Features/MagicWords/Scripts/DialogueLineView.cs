using Common.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MagicWords
{
	internal sealed class DialogueLineView : MonoBehaviour
	{
		private const float AvatarSize = 56f;
		private const float RowGap = 12f;
		private const float SidePad = 8f;
		private const float BubbleWidthFactor = 0.72f;

		[SerializeField]
		private AvatarView _avatar;

		[SerializeField]
		private RectTransform _avatarSlot;

		[SerializeField]
		private LayoutElement _avatarLayout;

		[SerializeField]
		private TMP_Text _speakerLabel;

		[SerializeField]
		private DialogueRenderer _bodyRenderer;

		[SerializeField]
		private Image _bubble;

		[SerializeField]
		private RectTransform _bubbleRoot;

		[SerializeField]
		private HorizontalLayoutGroup _rowLayout;

		[SerializeField]
		private LayoutElement _bubbleLayout;

		[SerializeField]
		private ContentSizeFitter _rootFitter;

		[SerializeField]
		private ContentSizeFitter _bubbleFitter;

		[SerializeField]
		private LayoutElement _spacer;

		private ResolvedDialogueLine _bound;
		private float _lastRowWidth = -1f;
		private bool _layoutConfigured;

		public AvatarView Avatar => _avatar;

		private void OnRectTransformDimensionsChange()
		{
			if (!isActiveAndEnabled || _bound == null)
			{
				return;
			}

			RefreshBubbleWidth(force: false);
		}

		public void Bind(ResolvedDialogueLine line)
		{
			_bound = line;
			if (line == null)
			{
				return;
			}

			_speakerLabel.text = string.IsNullOrWhiteSpace(line.Speaker)
				? MagicWordsTexts.UnknownSpeaker
				: line.Speaker;
			_speakerLabel.color = AppColors.AccentSoft;

			EnsureLayoutConfigured();
			ApplySide(line.Side);

			_bodyRenderer.Render(line.Tokens);
			_avatar.ShowPlaceholder(line.Speaker);
		}

		public void RefreshLayout()
		{
			if (_bound == null)
			{
				return;
			}

			RefreshBubbleWidth(force: true);
		}

		private void EnsureLayoutConfigured()
		{
			if (_layoutConfigured)
			{
				return;
			}

			_avatarLayout.minWidth = AvatarSize;
			_avatarLayout.minHeight = AvatarSize;
			_avatarLayout.preferredWidth = AvatarSize;
			_avatarLayout.preferredHeight = AvatarSize;
			_avatarLayout.flexibleWidth = 0f;
			_avatarLayout.flexibleHeight = 0f;
			_avatarLayout.ignoreLayout = false;

			_avatarSlot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, AvatarSize);
			_avatarSlot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, AvatarSize);

			_rowLayout.reverseArrangement = false;
			_rowLayout.childAlignment = TextAnchor.UpperLeft;
			_rowLayout.childForceExpandWidth = false;
			_rowLayout.childControlWidth = true;
			_rowLayout.childForceExpandHeight = false;
			_rowLayout.childControlHeight = true;
			_rowLayout.spacing = RowGap;
			_rowLayout.padding = new RectOffset((int)SidePad, (int)SidePad, 4, 4);

			_spacer.flexibleWidth = 1f;
			_spacer.minWidth = 0f;
			_spacer.preferredWidth = 0f;

			_bubbleLayout.flexibleWidth = 0f;
			_bubbleLayout.minWidth = 100f;
			_bubbleLayout.preferredWidth = -1f;

			_bubbleFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			_bubbleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			_rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			_rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			_layoutConfigured = true;
		}

		private void ApplySide(AvatarSide side)
		{
			var right = side == AvatarSide.Right;

			if (right)
			{
				_spacer.transform.SetSiblingIndex(0);
				_bubbleRoot.SetSiblingIndex(1);
				_avatarSlot.SetSiblingIndex(2);
			}
			else
			{
				_avatarSlot.SetSiblingIndex(0);
				_bubbleRoot.SetSiblingIndex(1);
				_spacer.transform.SetSiblingIndex(2);
			}

			_bubble.color = right ? AppColors.PanelSoft : AppColors.Panel;
			_speakerLabel.alignment = right ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
		}

		private void RefreshBubbleWidth(bool force)
		{
			var row = (RectTransform)transform;
			var rowWidth = row.rect.width;
			if (rowWidth <= 1f)
			{
				var parent = row.parent as RectTransform;
				if (parent != null)
				{
					rowWidth = parent.rect.width;
				}
			}

			if (!force && Mathf.Abs(rowWidth - _lastRowWidth) < 1f)
			{
				return;
			}

			_lastRowWidth = rowWidth;

			var reserved = AvatarSize + RowGap + SidePad * 2f + 16f;
			var available = Mathf.Max(120f, rowWidth - reserved);
			var maxBubble = Mathf.Min(available, rowWidth * BubbleWidthFactor);
			_bodyRenderer.SetMaxWidth(maxBubble);
		}
	}
}
