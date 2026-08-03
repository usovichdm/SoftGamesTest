using System.Collections.Generic;
using System.Text;
using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Turns dialogue tokens into one wrapping TMP label (text + inline emoji sprites).
    /// Sizes like a chat bubble: hug content, wrap at max width.
    /// </summary>
    public sealed class DialogueRenderer : MonoBehaviour
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
        private TMP_SpriteAsset _emojiSprites;

        private readonly StringBuilder _builder = new StringBuilder(128);

        private void Awake()
        {
            ApplyLabelStyle();
        }

        public void Render(IReadOnlyList<DialogueToken> tokens)
        {
            ApplyLabelStyle();
            _label.text = BuildRichText(tokens);
            FitBubbleWidth();
        }

        public void SetMaxWidth(float maxWidth)
        {
            _maxWidth = Mathf.Max(_minWidth, maxWidth);
            FitBubbleWidth();
        }

        public void Refit()
        {
            FitBubbleWidth();
        }

        private void ApplyLabelStyle()
        {
            _label.fontSize = _fontSize;
            _label.color = AppColors.TextPrimary;
            _label.alignment = TextAlignmentOptions.TopLeft;
            _label.textWrappingMode = TextWrappingModes.Normal;
            _label.overflowMode = TextOverflowModes.Overflow;
            _label.raycastTarget = false;
            _label.font = _font;
            _label.spriteAsset = _emojiSprites;
        }

        private void FitBubbleWidth()
        {
            var natural = _label.GetPreferredValues(_label.text);
            var width = Mathf.Clamp(natural.x, _minWidth, _maxWidth);
            var wrapped = _label.GetPreferredValues(_label.text, width, 0f);

            _layoutElement.minWidth = _minWidth;
            _layoutElement.preferredWidth = width;
            _layoutElement.flexibleWidth = 0f;
            _layoutElement.minHeight = wrapped.y;
            _layoutElement.preferredHeight = wrapped.y;
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
                switch (tokens[i])
                {
                    case TextToken text:
                        _builder.Append(text.Value);
                        break;
                    case EmojiToken emoji:
                        if (!string.IsNullOrEmpty(emoji.AtlasId))
                        {
                            _builder.Append("<sprite name=\"");
                            _builder.Append(emoji.AtlasId);
                            _builder.Append("\">");
                        }
                        else if (!string.IsNullOrEmpty(emoji.Unicode))
                        {
                            _builder.Append(emoji.Unicode);
                        }

                        break;
                }
            }

            return _builder.ToString();
        }
    }
}
