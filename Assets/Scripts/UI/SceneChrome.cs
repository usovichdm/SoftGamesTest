using SoftGames.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.UI
{
    /// <summary>
    /// Shared scene chrome: Menu button + FPS readout + SceneLoader.
    /// </summary>
    [RequireComponent(typeof(SceneLoader))]
    public sealed class SceneChrome : MonoBehaviour
    {
        [SerializeField]
        private Button _menuButton;

        [SerializeField]
        private FpsCounter _fpsCounter;

        [SerializeField]
        private SceneLoader _sceneLoader;

        private void Awake()
        {
            if (_sceneLoader == null)
            {
                _sceneLoader = GetComponent<SceneLoader>();
            }

            _menuButton.onClick.RemoveAllListeners();
            _menuButton.onClick.AddListener(() => _sceneLoader.LoadMainMenu());
        }
    }
}
