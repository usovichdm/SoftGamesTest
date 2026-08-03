using UnityEngine;
using UnityEngine.UI;

namespace SoftGames.UI
{
    /// <summary>
    /// Applies CanvasScaler defaults and safe-area padding for mobile notches.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class ResponsiveCanvas : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _safeAreaRoot;

        [SerializeField]
        private Vector2 _referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        private float _matchWidthOrHeight = 0.5f;

        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = _matchWidthOrHeight;

            ApplySafeArea();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea
                || _lastScreen.x != Screen.width
                || _lastScreen.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            _lastSafeArea = Screen.safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            var area = Screen.safeArea;
            var width = Mathf.Max(Screen.width, 1);
            var height = Mathf.Max(Screen.height, 1);

            _safeAreaRoot.anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            _safeAreaRoot.anchorMax = new Vector2(area.xMax / width, area.yMax / height);
            _safeAreaRoot.offsetMin = Vector2.zero;
            _safeAreaRoot.offsetMax = Vector2.zero;
        }
    }
}
