using TMPro;
using UnityEngine;

namespace SoftGames.UI
{
    /// <summary>
    /// Loading overlay layer driven by a prefab (dim + spinner + status text).
    /// </summary>
    public sealed class LoadingController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _spinTransform;

        [SerializeField]
        private CanvasGroup _group;

        [SerializeField]
        private TMP_Text _label;

        [SerializeField]
        private float _degreesPerSecond = 360f;

        private bool _active;

        public bool IsActive
        {
            get { return _active; }
        }

        /// <summary>
        /// Instantiates the overlay under the host canvas and starts hidden.
        /// </summary>
        public static LoadingController Spawn(LoadingController prefab, Transform host)
        {
            var parent = host.GetComponentInParent<Canvas>();
            var instance = Instantiate(prefab, parent != null ? parent.transform : host, false);
            instance.Hide();
            return instance;
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            _spinTransform.Rotate(0f, 0f, -_degreesPerSecond * Time.unscaledDeltaTime);
        }

        public void Show(string message = null)
        {
            _label.text = message ?? string.Empty;
            _active = true;
            gameObject.SetActive(true);
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
        }

        public void Hide()
        {
            _active = false;
            _label.text = string.Empty;
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}
