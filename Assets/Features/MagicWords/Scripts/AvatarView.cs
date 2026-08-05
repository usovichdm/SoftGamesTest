using Common.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.MagicWords
{
	internal sealed class AvatarView : MonoBehaviour
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

		private void OnDestroy()
		{
			ReleaseResources();
		}

		public void ShowPlaceholder(string speakerName)
		{
			ReleaseResources();

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

			ReleaseResources();

			_runtimeSprite = Sprite.Create(
				texture,
				new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f),
				100f);
			_runtimeSprite.name = "Avatar_" + (speakerName ?? MagicWordsTexts.UnknownSpeaker);

			_image.sprite = _runtimeSprite;
			_image.color = Color.white;
			_image.enabled = true;
			_image.preserveAspect = true;

			_initialLabel.gameObject.SetActive(false);

			_frame.enabled = true;
			_frame.color = AppColors.AccentSoft;
		}

		public void ReleaseResources()
		{
			if (_runtimeSprite == null)
			{
				return;
			}

			if (_image.sprite == _runtimeSprite)
			{
				_image.sprite = _placeholderSprite;
			}

			Destroy(_runtimeSprite);
			_runtimeSprite = null;
		}

		private static string GetInitial(string name)
		{
			return string.IsNullOrWhiteSpace(name) ? "?" : name.Trim()[..1].ToUpperInvariant();
		}
	}
}
