using UnityEngine;

namespace SoftGames.Gameplay.AceOfShadows
{
    /// <summary>
    /// Fits the pile row into the safe area for both portrait and landscape.
    /// Anchors to the top and scales down when vertical space is tight.
    /// </summary>
    public sealed class AcePilesLayout : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _pilesRoot;

        [SerializeField]
        private float _designWidth = 920f;

        [SerializeField]
        private float _designHeight = 520f;

        [SerializeField]
        private float _topMargin = 0.12f;

        [SerializeField]
        private float _bottomMargin = 0.06f;

        [SerializeField]
        private float _sideMargin = 0.04f;

        [SerializeField]
        private float _minScale = 0.42f;

        private Vector2Int _lastScreen;
        private Vector2 _lastParentSize;

        private void Awake()
        {
            if (_pilesRoot == null)
            {
                _pilesRoot = (RectTransform)transform;
            }
        }

        private void OnEnable()
        {
            Apply();
        }

        private void LateUpdate()
        {
            var parent = _pilesRoot.parent as RectTransform;
            var parentSize = parent != null ? parent.rect.size : Vector2.zero;

            if (_lastScreen.x == Screen.width
                && _lastScreen.y == Screen.height
                && _lastParentSize == parentSize)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            var parent = _pilesRoot.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            _lastParentSize = parent.rect.size;

            var availableWidth = parent.rect.width * (1f - _sideMargin * 2f);
            var availableHeight = parent.rect.height * (1f - _topMargin - _bottomMargin);
            if (availableWidth <= 1f || availableHeight <= 1f)
            {
                return;
            }

            var scale = Mathf.Min(availableWidth / _designWidth, availableHeight / _designHeight);
            scale = Mathf.Clamp(scale, _minScale, 1f);

            _pilesRoot.anchorMin = new Vector2(0.5f, 1f - _topMargin);
            _pilesRoot.anchorMax = new Vector2(0.5f, 1f - _topMargin);
            _pilesRoot.pivot = new Vector2(0.5f, 1f);
            _pilesRoot.anchoredPosition = Vector2.zero;
            _pilesRoot.sizeDelta = new Vector2(_designWidth, _designHeight);
            _pilesRoot.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
