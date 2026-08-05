using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.Core
{
	public static class SceneLoader
	{
		private const string MainMenu = "MainMenu";
		private const string AceOfShadows = "AceOfShadows";
		private const string MagicWords = "MagicWords";
		private const string PhoenixFlame = "PhoenixFlame";

		public static void LoadMainMenu()
		{
			Load(MainMenu);
		}

		public static void LoadAceOfShadows()
		{
			Load(AceOfShadows);
		}

		public static void LoadMagicWords()
		{
			Load(MagicWords);
		}

		public static void LoadPhoenixFlame()
		{
			Load(PhoenixFlame);
		}

		private static void Load(string sceneName)
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
