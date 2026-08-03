using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoftGames.Core
{
    /// <summary>
    /// Thin scene navigation helper for menu buttons and back navigation.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        public const string MainMenu = "MainMenu";
        public const string AceOfShadows = "AceOfShadows";
        public const string MagicWords = "MagicWords";
        public const string PhoenixFlame = "PhoenixFlame";

        public void LoadMainMenu()
        {
            Load(MainMenu);
        }

        public void LoadAceOfShadows()
        {
            Load(AceOfShadows);
        }

        public void LoadMagicWords()
        {
            Load(MagicWords);
        }

        public void LoadPhoenixFlame()
        {
            Load(PhoenixFlame);
        }

        public void Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoader] Scene name is empty.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
