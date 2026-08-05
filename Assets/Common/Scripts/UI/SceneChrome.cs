using Common.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
	internal sealed class SceneChrome : MonoBehaviour
	{
		[SerializeField]
		private Button _menuButton;

		private void Awake()
		{
			_menuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
		}

		private void OnDestroy()
		{
			_menuButton.onClick.RemoveAllListeners();
		}
	}
}
