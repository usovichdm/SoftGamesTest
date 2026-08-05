using Common.Core;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MagicWords
{
	internal sealed class DialogueRenderer : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _label;

		[SerializeField]
		private LayoutElement _layoutElement;

		[SerializeField]
		private float _fontSize = 20f;

		[SerializeField]
		private float _maxWidth = 460f;

		[SerializeField]
		private float _minWidth = 40f;

		[SerializeField]
		private TMP_FontAsset _font;

		[SerializeField]
		private EmojiCatalogAsset _emojiCatalog;

		private readonly StringBuilder _builder = new StringBuilder(128);

		private float _appliedWidth = -1f;
		private float _appliedHeight = -1f;
		private bool _styleApplied;

		private void Awake()
		{
			ApplyLabelStyle();
		}

		public void Render(IReadOnlyList<DialogueToken> tokens)
		{
			ApplyLabelStyle();
			_label.text = BuildRichText(tokens);
			_appliedWidth = -1f;
			_appliedHeight = -1f;
			FitBubbleWidth();
		}

		public void SetMaxWidth(float maxWidth)
		{
			var clamped = Mathf.Max(_minWidth, maxWidth);
			if (Mathf.Abs(clamped - _maxWidth) < 0.5f)
			{
				return;
			}

			_maxWidth = clamped;
			_appliedWidth = -1f;
			_appliedHeight = -1f;
			FitBubbleWidth();
		}

		private void ApplyLabelStyle()
		{
			if (_styleApplied)
			{
				return;
			}

			_label.fontSize = _fontSize;
			_label.color = AppColors.TextPrimary;
			_label.alignment = TextAlignmentOptions.TopLeft;
			_label.textWrappingMode = TextWrappingModes.Normal;
			_label.overflowMode = TextOverflowModes.Overflow;
			_label.raycastTarget = false;
			_label.font = _font;
			_label.spriteAsset = _emojiCatalog != null ? _emojiCatalog.SpriteAsset : null;
			_styleApplied = true;
		}

		private void FitBubbleWidth()
		{
			var natural = _label.GetPreferredValues(_label.text);
			var width = Mathf.Clamp(natural.x, _minWidth, _maxWidth);
			var height = _label.GetPreferredValues(_label.text, width, 0f).y;

			if (Mathf.Abs(width - _appliedWidth) < 0.5f
			    && Mathf.Abs(height - _appliedHeight) < 0.5f)
			{
				return;
			}

			_appliedWidth = width;
			_appliedHeight = height;

			_layoutElement.minWidth = _minWidth;
			_layoutElement.preferredWidth = width;
			_layoutElement.flexibleWidth = 0f;
			_layoutElement.minHeight = height;
			_layoutElement.preferredHeight = height;
		}

		private string BuildRichText(IReadOnlyList<DialogueToken> tokens)
		{
			_builder.Clear();
			if (tokens == null || tokens.Count == 0)
			{
				return string.Empty;
			}

			for (var i = 0; i < tokens.Count; i++)
			{
				var token = tokens[i];
				switch (token.Kind)
				{
					case DialogueTokenKind.Text:
						_builder.Append(token.Text);
						break;
					case DialogueTokenKind.Emoji:
						if (!string.IsNullOrEmpty(token.AtlasId))
						{
							_builder.Append("<sprite name=\"");
							_builder.Append(token.AtlasId);
							_builder.Append("\">");
						}
						else if (!string.IsNullOrEmpty(token.Unicode))
						{
							_builder.Append(token.Unicode);
						}

						break;
				}
			}

			return _builder.ToString();
		}
	}
}
