using Common.Core;
using TMPro;
using UnityEngine;

namespace Common.UI
{
	[DisallowMultipleComponent]
	internal sealed class FpsCounter : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _label;

		[SerializeField]
		private float _refreshSeconds = 0.5f;

		private float _elapsed;
		private int _frames;
		private float _accumulated;
		private int _lastShownFps = -1;

		private void Awake()
		{
			_label.color = AppColors.TextMuted;
		}

		private void Update()
		{
			_frames++;
			_accumulated += Time.unscaledDeltaTime;
			_elapsed += Time.unscaledDeltaTime;

			if (_elapsed < _refreshSeconds)
			{
				return;
			}

			var fps = Mathf.RoundToInt(_frames / Mathf.Max(_accumulated, 0.0001f));

			if (fps != _lastShownFps)
			{
				_lastShownFps = fps;
				_label.SetText("{0:0} FPS", fps);
			}

			_elapsed = 0f;
			_frames = 0;
			_accumulated = 0f;
		}
	}
}
