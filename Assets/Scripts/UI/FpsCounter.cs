using SoftGames.Core;
using TMPro;
using UnityEngine;

namespace SoftGames.UI
{
    /// <summary>
    /// Top-left FPS readout. Updates on an interval to avoid per-frame string alloc thrash.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FpsCounter : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _label;

        [SerializeField]
        private float _refreshSeconds = 0.25f;

        private float _elapsed;
        private int _frames;
        private float _accumulated;

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

            var fps = _frames / Mathf.Max(_accumulated, 0.0001f);
            _label.SetText("{0:0} FPS", fps);

            _elapsed = 0f;
            _frames = 0;
            _accumulated = 0f;
        }
    }
}
