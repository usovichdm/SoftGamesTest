using Common.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
	internal sealed class MainMenuController : MonoBehaviour
	{
		[SerializeField]
		private Button _aceButton;

		[SerializeField]
		private Button _magicButton;

		[SerializeField]
		private Button _phoenixButton;

		[SerializeField]
		private TMP_Text _title;

		[SerializeField]
		private TMP_Text _subtitle;

		[SerializeField]
		private Image _background;

		[SerializeField]
		private Image _accentBar;

		private void Awake()
		{
			_background.color = AppColors.BackgroundDeep;
			_accentBar.color = AppColors.Accent;

			_title.text = "SOFTGAMES";
			_title.color = AppColors.TextPrimary;

			_subtitle.text = "Unity Developer Assignment";
			_subtitle.color = AppColors.TextMuted;

			Subscribe();
		}

		private void OnDestroy()
		{
			Unsubscribe();
		}

		private void Subscribe()
		{
			_aceButton.onClick.AddListener(SceneLoader.LoadAceOfShadows);
			_magicButton.onClick.AddListener(SceneLoader.LoadMagicWords);
			_phoenixButton.onClick.AddListener(SceneLoader.LoadPhoenixFlame);
		}

		private void Unsubscribe()
		{
			_aceButton.onClick.RemoveAllListeners();
			_magicButton.onClick.RemoveAllListeners();
			_phoenixButton.onClick.RemoveAllListeners();
		}
	}
}
