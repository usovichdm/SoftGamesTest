using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.Gameplay.MagicWords
{
    /// <summary>
    /// Avatar image with graceful fallback when download/data fails.
    /// Always keeps a visible plate so landscape chat still shows faces/initials.
    /// </summary>
    public sealed class AvatarView : MonoBehaviour
    {
        [SerializeField]
        private Image _image;

        [SerializeField]
        private TMP_Text _initialLabel;

        [SerializeField]
        private Image _frame;

        [SerializeField]
        private Sprite _placeholderSprite;

        [SerializeField]
        private Color _placeholderTint = new Color(0.22f, 0.36f, 0.38f, 1f);

        private Sprite _runtimeSprite;

        private void Awake()
        {
            if (_image.sprite == null)
            {
                _image.sprite = _placeholderSprite;
            }
        }

        private void OnDestroy()
        {
            ReleaseRuntimeSprite();
        }

        public void ShowPlaceholder(string speakerName)
        {
            ReleaseRuntimeSprite();

            _image.sprite = _placeholderSprite;
            _image.color = _placeholderTint;
            _image.enabled = true;
            _image.preserveAspect = true;

            _initialLabel.gameObject.SetActive(true);
            _initialLabel.color = AppColors.TextPrimary;
            _initialLabel.text = GetInitial(speakerName);

            _frame.enabled = true;
            _frame.color = AppColors.AccentSoft;
        }

        public void ShowTexture(Texture2D texture, string speakerName)
        {
            if (texture == null)
            {
                ShowPlaceholder(speakerName);
                return;
            }

            ReleaseRuntimeSprite();

            _runtimeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            _runtimeSprite.name = "Avatar_" + (speakerName ?? "Unknown");

            _image.sprite = _runtimeSprite;
            _image.color = Color.white;
            _image.enabled = true;
            _image.preserveAspect = true;

            _initialLabel.gameObject.SetActive(false);

            _frame.enabled = true;
            _frame.color = AppColors.AccentSoft;
        }

        /// <summary>Drops runtime sprites before textures owned by the downloader are cleared.</summary>
        public void ReleaseResources()
        {
            ReleaseRuntimeSprite();
        }

        private void ReleaseRuntimeSprite()
        {
            if (_runtimeSprite == null)
            {
                return;
            }

            if (_image != null && _image.sprite == _runtimeSprite)
            {
                _image.sprite = _placeholderSprite;
            }

            // Immediate: ClearLines destroys downloader textures in the same call.
            DestroyImmediate(_runtimeSprite);
            _runtimeSprite = null;
        }

        private static string GetInitial(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            return name.Trim().Substring(0, 1).ToUpperInvariant();
        }
    }
}
