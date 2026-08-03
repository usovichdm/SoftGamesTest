using SoftGames.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.UI
{
    /// <summary>
    /// Main menu wiring: title polish + scene buttons.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        private SceneLoader _sceneLoader;

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

            _aceButton.onClick.RemoveAllListeners();
            _aceButton.onClick.AddListener(() => _sceneLoader.LoadAceOfShadows());

            _magicButton.onClick.RemoveAllListeners();
            _magicButton.onClick.AddListener(() => _sceneLoader.LoadMagicWords());

            _phoenixButton.onClick.RemoveAllListeners();
            _phoenixButton.onClick.AddListener(() => _sceneLoader.LoadPhoenixFlame());
        }
    }
}
